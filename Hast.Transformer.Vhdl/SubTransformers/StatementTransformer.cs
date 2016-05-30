using System;
using System.Linq;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Orchard;
using Orchard.Logging;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class StatementTransformer : IStatementTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly IExpressionTransformer _expressionTransformer;

        public ILogger Logger { get; set; }


        public StatementTransformer(ITypeConverter typeConverter, IExpressionTransformer expressionTransformer)
        {
            _typeConverter = typeConverter;
            _expressionTransformer = expressionTransformer;

            Logger = NullLogger.Instance;
        }


        public void Transform(Statement statement, ISubTransformerContext context)
        {
            TransformInner(statement, context);
        }


        private void TransformInner(Statement statement, ISubTransformerContext context)
        {
            var stateMachine = context.Scope.StateMachine;
            var currentBlock = context.Scope.CurrentBlock;

            if (statement is VariableDeclarationStatement)
            {
                var variableStatement = statement as VariableDeclarationStatement;

                var variableType = variableStatement.Type;
                var variableSimpleType = variableType as SimpleType;
                var isTaskFactory = false;

                // Filtering out variable declarations that were added by the compiler for multi-threaded code but
                // which shouldn't be transformed.
                var omitStatement =
                    // DisplayClass objects that generated for lambda expressions are put into variables like:
                    // PrimeCalculator.<>c__DisplayClass9_0 <>c__DisplayClass9_; They are being kept track of when
                    // processing the corresponding ObjectCreateExpressions.
                    variableType.GetFullName().IsDisplayClassName() ||
                    variableSimpleType != null &&
                    (
                        // The TaskFactory object is saved to a variable like TaskFactory arg_97_0;
                        (isTaskFactory = variableSimpleType.Identifier == nameof(System.Threading.Tasks.TaskFactory)) ||
                        // Delegates used for the body of Tasks are functions like: Func<object, bool> arg_97_1;
                        variableSimpleType.Identifier == "Func"
                    );
                if (!omitStatement)
                {
                    var type = _typeConverter.ConvertAstType(variableType);

                    foreach (var variableInitializer in variableStatement.Variables)
                    {
                        stateMachine.LocalVariables.Add(new Variable
                        {
                            Name = stateMachine.CreatePrefixedObjectName(variableInitializer.Name),
                            DataType = type
                        });
                    } 
                }
                else if (isTaskFactory)
                {
                    context.Scope.TaskFactoryVariableNames.Add(variableStatement.Variables.Single().Name);
                }
            }
            else if (statement is ExpressionStatement)
            {
                var expressionStatement = statement as ExpressionStatement;

                var expressionElement =
                    _expressionTransformer.Transform(expressionStatement.Expression, context)
                    .Terminate();
                currentBlock.Add(expressionElement);
            }
            else if (statement is ReturnStatement)
            {
                var returnStatement = statement as ReturnStatement;

                var returnType = _typeConverter.ConvertAstType(context.Scope.Method.ReturnType);
                if (returnType != KnownDataTypes.Void && returnType != SpecialTypes.Task)
                {
                    var assigmentElement = new Assignment
                    {
                        AssignTo = stateMachine.CreateReturnVariableName().ToVhdlVariableReference(),
                        Expression = _expressionTransformer.Transform(returnStatement.Expression, context)
                    };
                    currentBlock.Add(assigmentElement);
                }

                currentBlock.Add(stateMachine.ChangeToFinalState());
            }
            else if (statement is IfElseStatement)
            {
                var ifElse = statement as IfElseStatement;

                // Is this a compiler-generated if statement to create a Func for a DisplayClass method? Like:
                // if (arg_97_1 = <> c__DisplayClass9_.<> 9__0 == null) {
                //     arg_97_1 = <> c__DisplayClass9_.<> 9__0 = new Func<object, bool>(<> c__DisplayClass9_.< ParallelizedArePrimeNumbers > b__0);
                // }
                var scope = context.Scope;
                var isDisplayClassMethodReferenceCreatingIf =
                    ifElse.Condition.Is<BinaryOperatorExpression>(binary =>
                        binary.Left.Is<AssignmentExpression>(assignment => 
                            assignment.Right.Is<MemberReferenceExpression>(member => 
                                member.Target.Is<IdentifierExpression>(identifier =>
                                    scope.VariableToDisplayClassMappings.ContainsKey(identifier.Identifier)))) &&
                        binary.Right is NullReferenceExpression);
                if (isDisplayClassMethodReferenceCreatingIf)
                {
                    // There is only one child, an ExpressionStatement, which in turn also has a single child, an
                    // AssignmentExpression.
                    var assignment = (AssignmentExpression)ifElse.TrueStatement.Children.Single().Children.Single();

                    // Drilling into the expression to find out which DisplayClass method the Func referes to.
                    var funcCreateExpression = (ObjectCreateExpression)((AssignmentExpression)assignment.Right).Right;
                    var displayClassMemberReference = ((MemberReferenceExpression)funcCreateExpression.Arguments.Single());
                    var funcVariableName = ((IdentifierExpression)assignment.Left).Identifier;

                    scope.VariableToDisplayClassMethodMappings[funcVariableName] = displayClassMemberReference.GetFullName();
                }
                else
                {
                    // If-elses are always split up into multiple states, i.e. the true and false statements branch off
                    // into separate states. This makes it simpler to track how many clock cycles something requires, 
                    // since the latency of the two branches should be tracked separately.

                    var ifElseElement = new IfElse { Condition = _expressionTransformer.Transform(ifElse.Condition, context) };
                    var ifElseCommentsBlock = new LogicalBlock();
                    currentBlock.Add(new InlineBlock(ifElseCommentsBlock, ifElseElement));

                    var ifElseStartStateIndex = currentBlock.CurrentStateMachineStateIndex;
                    Func<int, IVhdlGenerationOptions, string> stateNameGenerator = (index, vhdlGenerationOptions) =>
                        vhdlGenerationOptions.NameShortener(stateMachine.CreateStateName(index));

                    var afterIfElseStateBlock = new InlineBlock(
                        new GeneratedComment(vhdlGenerationOptions =>
                            "State after the if-else which was started in state " +
                            stateNameGenerator(ifElseStartStateIndex, vhdlGenerationOptions) +
                            "."));
                    var afterIfElseStateIndex = stateMachine.AddState(afterIfElseStateBlock);

                    Func<IVhdlElement> createConditionalStateChangeToAfterIfElseState = () =>
                        new InlineBlock(
                            new GeneratedComment(vhdlGenerationOptions =>
                                "Going to the state after the if-else which was started in state " +
                                stateNameGenerator(ifElseStartStateIndex, vhdlGenerationOptions) +
                                "."),
                            CreateConditionalStateChange(afterIfElseStateIndex, context));


                    var trueStateBlock = new InlineBlock(
                        new GeneratedComment(vhdlGenerationOptions =>
                            "True branch of the if-else started in state " +
                            stateNameGenerator(ifElseStartStateIndex, vhdlGenerationOptions) +
                            "."));
                    var trueStateIndex = stateMachine.AddState(trueStateBlock);
                    ifElseElement.True = stateMachine.CreateStateChange(trueStateIndex);
                    currentBlock.ChangeBlockToDifferentState(trueStateBlock, trueStateIndex);
                    TransformInner(ifElse.TrueStatement, context);
                    currentBlock.Add(createConditionalStateChangeToAfterIfElseState());
                    var trueEndStateIndex = currentBlock.CurrentStateMachineStateIndex;

                    var falseStateIndex = 0;
                    var falseEndStateIndex = 0;
                    if (ifElse.FalseStatement != Statement.Null)
                    {
                        var falseStateBlock = new InlineBlock(
                            new GeneratedComment(vhdlGenerationOptions =>
                                "False branch of the if-else started in state " +
                                stateNameGenerator(ifElseStartStateIndex, vhdlGenerationOptions) +
                                "."));
                        falseStateIndex = stateMachine.AddState(falseStateBlock);
                        ifElseElement.Else = stateMachine.CreateStateChange(falseStateIndex);
                        currentBlock.ChangeBlockToDifferentState(falseStateBlock, falseStateIndex);
                        TransformInner(ifElse.FalseStatement, context);
                        currentBlock.Add(createConditionalStateChangeToAfterIfElseState());
                        falseEndStateIndex = currentBlock.CurrentStateMachineStateIndex;
                    }
                    else
                    {
                        ifElseElement.Else = new InlineBlock(
                            new LineComment("There was no false branch, so going directly to the state after the if-else."),
                            stateMachine.CreateStateChange(afterIfElseStateIndex));
                    }


                    ifElseCommentsBlock.Add(
                        new LineComment("This if-else was transformed from a .NET if-else. It spans across multiple states:"));
                    ifElseCommentsBlock.Add(new GeneratedComment(vhdlGenerationOptions =>
                        "    * The true branch starts in state " + stateNameGenerator(trueStateIndex, vhdlGenerationOptions) +
                        " and ends in state " + stateNameGenerator(trueEndStateIndex, vhdlGenerationOptions) + "."));
                    if (falseStateIndex != 0)
                    {
                        ifElseCommentsBlock.Add(new GeneratedComment(vhdlGenerationOptions =>
                            "    * The false branch starts in state " + stateNameGenerator(falseStateIndex, vhdlGenerationOptions) +
                            " and ends in state " + stateNameGenerator(falseEndStateIndex, vhdlGenerationOptions) + "."));
                    }
                    ifElseCommentsBlock.Add(new GeneratedComment(vhdlGenerationOptions =>
                        "    * Execution after either branch will continue in the following state: " +
                        stateNameGenerator(afterIfElseStateIndex, vhdlGenerationOptions) + "."));


                    currentBlock.ChangeBlockToDifferentState(afterIfElseStateBlock, afterIfElseStateIndex); 
                }
            }
            else if (statement is BlockStatement)
            {
                var blockStatement = statement as BlockStatement;

                foreach (var blockStatementStatement in blockStatement.Statements)
                {
                    TransformInner(blockStatementStatement, context);
                }
            }
            else if (statement is WhileStatement)
            {
                var whileStatement = statement as WhileStatement;

                var whileStartStateIndex = currentBlock.CurrentStateMachineStateIndex;
                Func<IVhdlGenerationOptions, string> whileStartStateIndexNameGenerator = vhdlGenerationOptions =>
                    vhdlGenerationOptions.NameShortener(stateMachine.CreateStateName(whileStartStateIndex));

                var repeatedState = new InlineBlock(
                    new GeneratedComment(vhdlGenerationOptions => 
                        "Repeated state of the while loop which was started in state " + 
                        whileStartStateIndexNameGenerator(vhdlGenerationOptions) + 
                        "."));
                var repeatedStateIndex = stateMachine.AddState(repeatedState);
                var afterWhileState = new InlineBlock(
                    new GeneratedComment(vhdlGenerationOptions => 
                        "State after the while loop which was started in state " + 
                        whileStartStateIndexNameGenerator(vhdlGenerationOptions) + 
                        "."));
                var afterWhileStateIndex = stateMachine.AddState(afterWhileState);

                // Having a condition even in the current state's body: if the loop doesn't need to run at all we'll
                // spare one cycle by directly jumping to the state after the loop.
                currentBlock.Add(new LineComment("Starting a while loop."));
                currentBlock.Add(new LineComment(
                    "The while loop's condition (also added here to be able to branch off early if the loop body shouldn't be executed at all):"));
                currentBlock.Add(new IfElse
                    {
                        Condition = _expressionTransformer.Transform(whileStatement.Condition, context),
                        True = stateMachine.CreateStateChange(repeatedStateIndex),
                        Else = stateMachine.CreateStateChange(afterWhileStateIndex)
                    });

                var whileStateInnerBody = new InlineBlock();

                currentBlock.ChangeBlockToDifferentState(repeatedState, repeatedStateIndex);
                repeatedState.Add(new LineComment("The while loop's condition:"));
                repeatedState.Add(new IfElse
                    {
                        Condition = _expressionTransformer.Transform(whileStatement.Condition, context),
                        True = whileStateInnerBody,
                        Else = stateMachine.CreateStateChange(afterWhileStateIndex)
                    });

                currentBlock.ChangeBlock(whileStateInnerBody);
                TransformInner(whileStatement.EmbeddedStatement, context);

                // Returning to the state of the while condition so the cycle can re-start.
                var lastState = stateMachine.States.Last().Body;
                if (lastState != afterWhileState)
                {
                    // We need an if to check whether the state was changed in the logic. If it was then that means
                    // that the loop was exited so we mustn't overwrite the new state.
                    currentBlock.Add(
                        new GeneratedComment(vhdlGenerationOptions => 
                            "Returning to the repeated state of the while loop which was started in state " +
                            whileStartStateIndexNameGenerator(vhdlGenerationOptions) + 
                            " if the loop wasn't exited with a state change."));

                    currentBlock.Add(CreateConditionalStateChange(repeatedStateIndex, context));
                }
                currentBlock.ChangeBlockToDifferentState(afterWhileState, afterWhileStateIndex);
            }
            else if (statement is ThrowStatement)
            {
                Logger.Warning("The exception throw statement \"{0}\" was omitted during transformation to be able to transform the code. However this can cause issues for certain algorithms; if it is an issue for this one then this code can't be transformed.", statement.ToString());
                currentBlock.Add(new LineComment("A throw statement was here, which was omitted during transformation."));
            }
            else throw new NotSupportedException("Statements of type " + statement.GetType() + " are not supported to be transformed to VHDL.");
        }

        /// <summary>
        /// Creates a conditional state change to the destination state that will only take place if the state wasn't
        /// already changed in the current state.
        /// </summary>
        private IVhdlElement CreateConditionalStateChange(int destinationStateIndex, ISubTransformerContext context)
        {
            // We need an if to check whether the state was changed in the logic. If it was then that means
            // that the subroutine was exited so we mustn't overwrite the new state.

            var stateMachine = context.Scope.StateMachine;

            return new IfElse
                {
                    Condition = new Binary
                    {
                        Left = stateMachine.CreateStateVariableName().ToVhdlVariableReference(),
                        Operator = BinaryOperator.Equality,
                        Right = stateMachine.CreateStateName(context.Scope.CurrentBlock.CurrentStateMachineStateIndex).ToVhdlIdValue()
                    },
                    True = stateMachine.CreateStateChange(destinationStateIndex)
                };
        }
    }
}
