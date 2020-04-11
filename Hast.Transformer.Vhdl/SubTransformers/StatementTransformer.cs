﻿using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class StatementTransformer : IStatementTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly IExpressionTransformer _expressionTransformer;
        private readonly IDeclarableTypeCreator _declarableTypeCreator;
        private readonly ITypeConversionTransformer _typeConversionTransformer;


        public StatementTransformer(
            ITypeConverter typeConverter,
            IExpressionTransformer expressionTransformer,
            IDeclarableTypeCreator declarableTypeCreator,
            ITypeConversionTransformer typeConversionTransformer)
        {
            _typeConverter = typeConverter;
            _expressionTransformer = expressionTransformer;
            _declarableTypeCreator = declarableTypeCreator;
            _typeConversionTransformer = typeConversionTransformer;
        }


        public void Transform(Statement statement, ISubTransformerContext context)
        {
            TransformInner(statement, context);
        }


        private void TransformInner(Statement statement, ISubTransformerContext context)
        {
            var scope = context.Scope;
            var stateMachine = scope.StateMachine;
            var currentBlock = scope.CurrentBlock;

            string stateNameGenerator(int index, IVhdlGenerationOptions vhdlGenerationOptions) =>
                vhdlGenerationOptions.NameShortener(stateMachine.CreateStateName(index));

            currentBlock.Add(new LineComment("The following section was transformed from the .NET statement below:"));
            currentBlock.Add(new BlockComment(statement.ToString()));

            if (statement is VariableDeclarationStatement variableStatement)
            {
                var variableType = variableStatement.Type;
                var variableSimpleType = variableType as SimpleType;
                var isTaskFactory = false;

                // Filtering out variable declarations that were added by the compiler for multi-threaded code but
                // which shouldn't be transformed.
                var omitStatement =
                    // DisplayClass objects that generated for lambda expressions are put into variables like:
                    // PrimeCalculator.<>c__DisplayClass9_0 <>c__DisplayClass9_; They are being kept track of when
                    // processing the corresponding ObjectCreateExpressions.
                    variableType.GetFullName().IsDisplayOrClosureClassName() ||
                    variableSimpleType != null &&
                    (
                        // The TaskFactory object is saved to a variable like TaskFactory arg_97_0;
                        (isTaskFactory = variableSimpleType.Identifier == nameof(System.Threading.Tasks.TaskFactory)) ||
                        // Delegates used for the body of Tasks are functions like: Func<object, bool> arg_97_1;
                        variableSimpleType.Identifier == "Func"
                    );
                if (!omitStatement)
                {
                    foreach (var variableInitializer in variableStatement.Variables)
                    {
                        stateMachine.LocalVariables.Add(new Variable
                        {
                            Name = stateMachine.CreatePrefixedObjectName(variableInitializer.Name),
                            DataType = _declarableTypeCreator.CreateDeclarableType(variableInitializer, variableType, context.TransformationContext)
                        });
                    }
                }
            }
            else if (statement is ExpressionStatement expressionStatement)
            {
                var expressionElement = _expressionTransformer.Transform(expressionStatement.Expression, context);

                // If the element is just a DataObjectReference (so e.g. a variable reference) alone then it needs to
                // be discarded. This can happen e.g. with calls to non-void methods where the return value is not
                // assigned: That causes the return value's reference to be orphaned.
                if (!(expressionElement is DataObjectReference))
                {
                    currentBlock.Add(expressionElement.Terminate());
                }
            }
            else if (statement is ReturnStatement returnStatement)
            {
                var returnType = _typeConverter.ConvertAstType(scope.Method.ReturnType, context.TransformationContext);
                if (returnType != KnownDataTypes.Void && returnType != SpecialTypes.Task)
                {
                    IDataObject returnReference = stateMachine.CreateReturnSignalReference();
                    IVhdlElement returnExpression = _expressionTransformer.Transform(returnStatement.Expression, context);

                    // It can happen that the type of the expression is not the same as the return type of the method.
                    // Thus a cast may be necessary.
                    var expressionType = returnStatement.Expression.GetActualType();
                    var expressionVhdlType = (returnExpression as Value)?.DataType ??
                        (expressionType != null ? _typeConverter.ConvertType(expressionType, context.TransformationContext) : null);
                    if (expressionVhdlType != null)
                    {
                        returnExpression = _typeConversionTransformer
                            .ImplementTypeConversion(expressionVhdlType, returnType, returnExpression)
                            .ConvertedFromExpression;

                    }

                    var assigmentElement = new Assignment
                    {
                        AssignTo = returnReference,
                        Expression = returnExpression
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
            else if (statement is IfElseStatement ifElse)
            {
                // If-elses are always split up into multiple states, i.e. the true and false statements branch off
                // into separate states. This makes it simpler to track how many clock cycles something requires, since
                // the latency of the two branches should be tracked separately.

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

                IVhdlElement createConditionalStateChangeToAfterIfElseState() =>
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
            else if (statement is BlockStatement blockStatement)
            {
                foreach (var blockStatementStatement in blockStatement.Statements)
                {
                    TransformInner(blockStatementStatement, context);
                }
            }
            else if (statement is WhileStatement whileStatement)
            {
                var whileStartStateIndex = currentBlock.StateMachineStateIndex;
                string whileStartStateIndexNameGenerator(IVhdlGenerationOptions vhdlGenerationOptions) =>
                    vhdlGenerationOptions.NameShortener(stateMachine.CreateStateName(whileStartStateIndex));

                var repeatedStateStart = new InlineBlock(
                    new GeneratedComment(vhdlGenerationOptions =>
                        "Repeated state of the while loop which was started in state " +
                        whileStartStateIndexNameGenerator(vhdlGenerationOptions) +
                        "."));
                var repeatedStateStartIndex = stateMachine.AddState(repeatedStateStart);
                var afterWhileState = new InlineBlock(
                    new GeneratedComment(vhdlGenerationOptions =>
                        "State after the while loop which was started in state " +
                        whileStartStateIndexNameGenerator(vhdlGenerationOptions) +
                        "."));
                var afterWhileStateIndex = stateMachine.AddState(afterWhileState);
                GetOrCreateAfterWhileStateIndexStack(context).Push(afterWhileStateIndex);

                currentBlock.Add(new LineComment("Starting a while loop."));
                currentBlock.Add(stateMachine.CreateStateChange(repeatedStateStartIndex));

                var whileStateInnerBody = new InlineBlock();

                currentBlock.ChangeBlockToDifferentState(repeatedStateStart, repeatedStateStartIndex);
                repeatedStateStart.Add(new LineComment("The while loop's condition:"));
                var conditionResultReference = _expressionTransformer.Transform(whileStatement.Condition, context);
                currentBlock.Add(new IfElse
                {
                    Condition = conditionResultReference,
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

                    currentBlock.Add(CreateConditionalStateChange(repeatedStateStartIndex, context));
                }
                currentBlock.ChangeBlockToDifferentState(afterWhileState, afterWhileStateIndex);

                GetOrCreateAfterWhileStateIndexStack(context).Pop();
            }
            else if (statement is ThrowStatement)
            {
                scope.Warnings.AddWarning(
                    "ThrowStatementOmitted",
                    "The exception throw statement \"" + statement.ToString() +
                    "\" was omitted during transformation to be able to transform the code. However this can cause issues for certain algorithms; if it is an issue for this one then this code can't be transformed.");

                currentBlock.Add(new LineComment("A throw statement was here, which was omitted during transformation."));
            }
            else if (statement is SwitchStatement switchStatement)
            {
                var caseStatement = new Case
                {
                    Expression = _expressionTransformer.Transform(switchStatement.Expression, context)
                };
                currentBlock.Add(caseStatement);


                // Case statements, much like if-else statements need a state added in advance where all branches will
                // will finally return to. All branches have their own states too.
                var caseStartStateIndex = currentBlock.StateMachineStateIndex;

                var afterCaseStateBlock = new InlineBlock(
                    new GeneratedComment(vhdlGenerationOptions =>
                        "State after the case statement which was started in state " +
                        stateNameGenerator(caseStartStateIndex, vhdlGenerationOptions) +
                        "."));
                var aftercaseStateIndex = stateMachine.AddState(afterCaseStateBlock);

                IVhdlElement createConditionalStateChangeToAfterCaseState() =>
                    new InlineBlock(
                        new GeneratedComment(vhdlGenerationOptions =>
                            "Going to the state after the case statement which was started in state " +
                            stateNameGenerator(caseStartStateIndex, vhdlGenerationOptions) +
                            "."),
                        CreateConditionalStateChange(aftercaseStateIndex, context));

                var switchExpressionType = _typeConverter
                    .ConvertType(switchStatement.Expression.GetActualType(), context.TransformationContext);

                foreach (var switchSection in switchStatement.SwitchSections)
                {
                    var when = new CaseWhen();
                    caseStatement.Whens.Add(when);
                    var whenBody = new InlineBlock();
                    when.Body.Add(whenBody);

                    scope.CurrentBlock.ChangeBlock(whenBody);
                    stateMachine.AddNewStateAndChangeCurrentBlock(scope);

                    // If there are multiple labels for a switch section then those should be OR-ed together.
                    when.Expression = BinaryChainBuilder.BuildBinaryChain(
                        switchSection.CaseLabels.Select(caseLabel =>
                        {
                            var caseExpressionType = _typeConverter
                                .ConvertType(caseLabel.Expression.GetActualType(), context.TransformationContext);

                            return _typeConversionTransformer.ImplementTypeConversion(
                                caseExpressionType,
                                switchExpressionType,
                                _expressionTransformer.Transform(caseLabel.Expression, context))
                            .ConvertedFromExpression;
                        }),
                        BinaryOperator.Or);

                    foreach (var sectionStatement in switchSection.Statements)
                    {
                        Transform(sectionStatement, context);
                    }

                    currentBlock.Add(createConditionalStateChangeToAfterCaseState());
                }

                // If the AST doesn't contain cases for all possible values of the type the statement switches on then
                // the VHDL will be incorrect. By including an "others" case every time this is solved.
                caseStatement.Whens.Add(CaseWhen.CreateOthers());


                currentBlock.ChangeBlockToDifferentState(afterCaseStateBlock, aftercaseStateIndex);
            }
            else if (statement is BreakStatement breakStatement)
            {
                // If this is a break in a switch's section then nothing to do: these are not needed in VHDL.
                if (statement.Parent is SwitchSection)
                {
                    return;
                }

                var afterWhileStack = GetOrCreateAfterWhileStateIndexStack(context);
                if (afterWhileStack.Any())
                {
                    currentBlock.Add(new LineComment("Exiting the while loop with a break statement."));
                    currentBlock.Add(stateMachine.CreateStateChange(afterWhileStack.Peek()));

                    return;
                }

                throw new NotSupportedException(
                    "Break statements outside of switch statements and loops are not supported.".AddParentEntityName(statement));
            }
            else if (statement is GotoStatement gotoStatement)
            {
                currentBlock.Add(stateMachine.CreateStateChange(scope.LabelsToStateIndicesMappings[gotoStatement.Label]));

                var newBlock = new InlineBlock();
                scope.CurrentBlock.ChangeBlockToDifferentState(newBlock, stateMachine.AddState(newBlock));
            }
            else if (statement is LabelStatement labelStatement)
            {
                var labelStateIndex = scope.LabelsToStateIndicesMappings[labelStatement.Label];
                scope.CurrentBlock.Add(stateMachine.CreateStateChange(labelStateIndex));
                currentBlock.ChangeBlockToDifferentState(stateMachine.States[labelStateIndex].Body, labelStateIndex);
            }
            else if (statement is EmptyStatement)
            {
                // Nothing to do.
            }
            else throw new NotSupportedException(
                "Statements of type " + statement.GetType() + " are not supported.".AddParentEntityName(statement));
        }

        /// <summary>
        /// Creates a conditional state change to the destination state that will only take place if the state wasn't
        /// already changed in the current state.
        /// </summary>
        private IVhdlElement CreateConditionalStateChange(int destinationStateIndex, ISubTransformerContext context)
        {
            // We need an if to check whether the state was changed in the logic. If it was then that means that the
            // subroutine was exited so we mustn't overwrite the new state.

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


        // Keeping track of the index of the state after while statements, so this can be used to break out of the loop.
        private static Stack<int> GetOrCreateAfterWhileStateIndexStack(ISubTransformerContext context)
        {
            var key = "Hast.Transformer.Vhdl.AfterWhileStateIndexStack";

            if (context.Scope.CustomProperties.TryGetValue(key, out var stack)) return stack;

            return context.Scope.CustomProperties[key] = new Stack<int>();
        }
    }
}
