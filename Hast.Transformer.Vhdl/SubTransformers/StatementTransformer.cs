using System;
using System.Linq;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
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
            var typeDeclarationLookupTable = context.TransformationContext.TypeDeclarationLookupTable;

            Func<int, IVhdlGenerationOptions, string> stateNameGenerator = (index, vhdlGenerationOptions) =>
                vhdlGenerationOptions.NameShortener(stateMachine.CreateStateName(index));

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
                    var type = _typeConverter.ConvertAstType(variableType, typeDeclarationLookupTable);

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

                var expressionElement = _expressionTransformer.Transform(expressionStatement.Expression, context);

                // If the element is just a DataObjectReference (so e.g. a variable reference) alone then it needs to
                // be discarded. This can happen e.g. with calls to non-void methods where the return value is not
                // assigned: That causes the return value's reference to be orphaned.
                if (!(expressionElement is DataObjectReference))
                {
                    currentBlock.Add(expressionElement.Terminate()); 
                }
            }
            else if (statement is ReturnStatement)
            {
                var returnStatement = statement as ReturnStatement;

                var returnType = _typeConverter.ConvertAstType(context.Scope.Method.ReturnType, typeDeclarationLookupTable);
                if (returnType != KnownDataTypes.Void && returnType != SpecialTypes.Task)
                {
                    var assigmentElement = new Assignment
                    {
                        AssignTo = stateMachine.CreateReturnSignalReference(),
                        Expression = _expressionTransformer.Transform(returnStatement.Expression, context)
                    };

                    // If the expression is an assignment we can't assign it to the return signal, so need to split it.
                    // This happens with lines like:
                    // return (Number += increaseBy);
                    if (assigmentElement.Expression is Assignment)
                    {
                        currentBlock.Add(assigmentElement.Expression);
                        assigmentElement.Expression = ((Assignment)assigmentElement.Expression).AssignTo;
                    }

                    currentBlock.Add(assigmentElement);
                }

                currentBlock.Add(stateMachine.ChangeToFinalState());
            }
            else if (statement is IfElseStatement)
            {
                var ifElse = statement as IfElseStatement;

                // Is this a compiler-generated if statement to create a Func for a DisplayClass method? Like:
                // if (arg_97_1 = <> c__DisplayClass9_.<> 9__0 == null) {
                //     arg_97_1 = <> c__DisplayClass9_.<> 9__0 = new Func<object, bool>(<>c__DisplayClass9_.<ParallelizedArePrimeNumbers>b__0);
                // }
                // Similar, but slightly different:
                // if (arg_42_1 = HastlayerOptimizedAlgorithm.<>c.<>9__3_0 == null) {
                //     arg_42_1 = HastlayerOptimizedAlgorithm.<>c.<> 9__3_0 = new Func<object, uint>(HastlayerOptimizedAlgorithm.<> c.<> 9.<Run>b__3_0);
                // }
                var scope = context.Scope;
                var isDisplayClassMethodReferenceCreatingIf =
                    ifElse.Condition.Is<BinaryOperatorExpression>(binary =>
                        binary.Left.Is<AssignmentExpression>(assignment =>
                            assignment.Right.Is<MemberReferenceExpression>(member =>
                                member.Target.Is<TypeReferenceExpression>(typeReference =>
                                    typeReference.Type.GetFullName().IsDisplayClassName())
                                ||
                                member.Target.Is<IdentifierExpression>(identifier =>
                                    scope.VariableNameToDisplayClassNameMappings.ContainsKey(identifier.Identifier)))) &&
                        binary.Right is NullReferenceExpression);
                if (isDisplayClassMethodReferenceCreatingIf)
                {
                    // There is only one child, an ExpressionStatement, which in turn also has a single child, an
                    // AssignmentExpression.
                    var assignment = (AssignmentExpression)ifElse.TrueStatement.Children.Single().Children.Single();

                    // Drilling into the expression to find out which DisplayClass method the Func referes to.
                    var funcCreateExpression = (ObjectCreateExpression)((AssignmentExpression)assignment.Right).Right;
                    var displayClassMemberReference = TaskParallelizationHelper
                        .GetTargetDisplayClassMemberFromFuncCreation(funcCreateExpression);
                    var funcVariableName = ((IdentifierExpression)assignment.Left).Identifier;

                    scope.FuncVariableNameToDisplayClassMethodMappings[funcVariableName] =
                        (MethodDeclaration)displayClassMemberReference
                        .GetMemberDeclaration(context.TransformationContext.TypeDeclarationLookupTable);
                }
                else
                {
                    // If-elses are always split up into multiple states, i.e. the true and false statements branch off
                    // into separate states. This makes it simpler to track how many clock cycles something requires, 
                    // since the latency of the two branches should be tracked separately.

                    var ifElseElement = new IfElse { Condition = _expressionTransformer.Transform(ifElse.Condition, context) };
                    var ifElseCommentsBlock = new LogicalBlock();
                    currentBlock.Add(new InlineBlock(ifElseCommentsBlock, ifElseElement));

                    var ifElseStartStateIndex = currentBlock.StateMachineStateIndex;

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
                    var trueEndStateIndex = currentBlock.StateMachineStateIndex;

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
                        falseEndStateIndex = currentBlock.StateMachineStateIndex;
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

                var whileStartStateIndex = currentBlock.StateMachineStateIndex;
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
            else if (statement is SwitchStatement)
            {
                var switchStatement = statement as SwitchStatement;


                var caseStatement = new Case
                {
                    Expression = _expressionTransformer.Transform(switchStatement.Expression, context)
                };
                currentBlock.Add(caseStatement);


                // Case statements, much like if-else statements need a state added in advance where all branches will
                // will finally return to.
                var caseStartStateIndex = currentBlock.StateMachineStateIndex;

                var afterCaseStateBlock = new InlineBlock(
                    new GeneratedComment(vhdlGenerationOptions =>
                        "State after the case statement which was started in state " +
                        stateNameGenerator(caseStartStateIndex, vhdlGenerationOptions) +
                        "."));
                var aftercaseStateIndex = stateMachine.AddState(afterCaseStateBlock);

                Func<IVhdlElement> createConditionalStateChangeToAfterCaseState = () =>
                    new InlineBlock(
                        new GeneratedComment(vhdlGenerationOptions =>
                            "Going to the state after the case statement which was started in state " +
                            stateNameGenerator(caseStartStateIndex, vhdlGenerationOptions) +
                            "."),
                        CreateConditionalStateChange(aftercaseStateIndex, context));


                foreach (var switchSection in switchStatement.SwitchSections)
                {
                    var when = new CaseWhen();
                    caseStatement.Whens.Add(when);

                    // If there are multiple labels for a switch section then those should be OR-ed together.
                    when.Expression = BinaryChainBuilder.BuildBinaryChain(
                        switchSection.CaseLabels.Select(caseLabel => _expressionTransformer.Transform(caseLabel.Expression, context)),
                        BinaryOperator.ConditionalOr);

                    var whenBody = new InlineBlock();
                    when.Body.Add(whenBody);
                    currentBlock.ChangeBlock(whenBody);

                    foreach (var sectionStatement in switchSection.Statements)
                    {
                        Transform(sectionStatement, context);
                    }

                    currentBlock.Add(createConditionalStateChangeToAfterCaseState());
                }


                currentBlock.ChangeBlockToDifferentState(afterCaseStateBlock, aftercaseStateIndex);
            }
            else if (statement is BreakStatement)
            {
                var breakStatement = statement as BreakStatement;
                
                // If this is a break in a switch's section then nothing to do: these are not needed in VHDL.
                if (statement.Parent is SwitchSection)
                {
                    return;
                }

                throw new NotSupportedException("Break statements outside of switch statements are not supported.");
            }
            else throw new NotSupportedException("Statements of type " + statement.GetType() + " are not supported.");
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
                    Right = stateMachine.CreateStateName(context.Scope.CurrentBlock.StateMachineStateIndex).ToVhdlIdValue()
                },
                True = stateMachine.CreateStateChange(destinationStateIndex)
            };
        }
    }
}
