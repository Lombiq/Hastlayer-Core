using System;
using System.Linq;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IStatementTransformer : IDependency
    {
        void Transform(Statement statement, ISubTransformerContext context);
    }


    public class StatementTransformer : IStatementTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly IExpressionTransformer _expressionTransformer;


        public StatementTransformer(ITypeConverter typeConverter, IExpressionTransformer expressionTransformer)
        {
            _typeConverter = typeConverter;
            _expressionTransformer = expressionTransformer;
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

                var type = _typeConverter.Convert(variableStatement.Type);

                foreach (var variableInitializer in variableStatement.Variables)
                {
                    stateMachine.LocalVariables.Add(new Variable
                    {
                        Name = stateMachine.CreatePrefixedVariableName(variableInitializer.Name),
                        DataType = type
                    });
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

                if (_typeConverter.Convert((context.Scope.Method).ReturnType) != KnownDataTypes.Void)
                {
                    var assigmentElement = new Assignment
                    {
                        AssignTo = stateMachine.CreateReturnVariableName().ToVhdlVariableReference(),
                        Expression = _expressionTransformer.Transform(returnStatement.Expression, context)
                    }.Terminate();
                    currentBlock.Add(assigmentElement);
                }

                currentBlock.Add(stateMachine.ChangeToFinalState());
            }
            else if (statement is IfElseStatement)
            {
                var ifElse = statement as IfElseStatement;

                var ifElseElement = new IfElse { Condition = _expressionTransformer.Transform(ifElse.Condition, context) };
                currentBlock.Add(ifElseElement);

                var trueBlock = new InlineBlock();
                ifElseElement.True = trueBlock;
                currentBlock.ChangeBlock(trueBlock);
                TransformInner(ifElse.TrueStatement, context);

                if (ifElse.FalseStatement != Statement.Null)
                {
                    var falseBlock = new InlineBlock();
                    ifElseElement.Else = falseBlock;
                    currentBlock.ChangeBlock(falseBlock);
                    TransformInner(ifElse.FalseStatement, context);
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

                var whileState = new InlineBlock();
                var whileStateIndex = stateMachine.AddState(whileState);
                var afterWhileState = new InlineBlock();
                var afterWhileStateIndex = stateMachine.AddState(afterWhileState);

                var condition = _expressionTransformer.Transform(whileStatement.Condition, context);

                // Having a condition even in the current state's body: if the loop doesn't need to run at all we'll
                // spare one cycle by directly jumping to the state after the loop.
                currentBlock.Add(new IfElse
                    {
                        Condition = condition,
                        True = stateMachine.CreateStateChange(whileStateIndex),
                        Else = stateMachine.CreateStateChange(afterWhileStateIndex)
                    });

                var whileStateInnerBody = new InlineBlock();
                currentBlock.ChangeBlock(whileStateInnerBody);
                TransformInner(whileStatement.EmbeddedStatement, context);

                whileState.Body.Add(new IfElse
                    {
                        Condition = condition,
                        True = whileStateInnerBody,
                        Else = stateMachine.CreateStateChange(afterWhileStateIndex)
                    });

                // Returning to the of the while condition so the cycle can re-start.
                stateMachine.States.Last().Body.Add(stateMachine.CreateStateChange(whileStateIndex));
            }
            else throw new NotSupportedException("Statements of type " + statement.GetType() + " are not supported to be transformed to VHDL.");
        }
    }
}
