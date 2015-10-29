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
        void Transform(Statement statement, ISubTransformerContext context, IBlockElement block);   
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


        public void Transform(Statement statement, ISubTransformerContext context, IBlockElement block)
        {
            var subProgram = context.Scope.SubProgram;

            if (statement is VariableDeclarationStatement)
            {
                var variableStatement = statement as VariableDeclarationStatement;

                var type = _typeConverter.Convert(variableStatement.Type);

                foreach (var variableInitializer in variableStatement.Variables)
                {
                    subProgram.Declarations.Add(new Variable
                    {
                        Name = variableInitializer.Name.ToExtendedVhdlId(),
                        DataType = type
                    });
                }
            }
            else if (statement is ExpressionStatement)
            {
                var expressionStatement = statement as ExpressionStatement;

                block.Body.Add(new Terminated(_expressionTransformer.Transform(expressionStatement.Expression, context, block)));
            }
            else if (statement is ReturnStatement)
            {
                var returnStatement = statement as ReturnStatement;

                if (_typeConverter.Convert((context.Scope.Method).ReturnType) == KnownDataTypes.Void)
                {
                    block.Body.Add(new Return());
                }
                else if (subProgram is Procedure)
                {
                    var procedure = (Procedure)subProgram;

                    var outputParam = procedure.Parameters.Where(param => param.ParameterType == ProcedureParameterType.Out).Single();

                    block.Body.Add(new Terminated(new Assignment
                    {
                        AssignTo = outputParam,
                        Expression = _expressionTransformer.Transform(returnStatement.Expression, context, block)
                    }));
                    block.Body.Add(new Return());
                }
            }
            else if (statement is IfElseStatement)
            {
                var ifElse = statement as IfElseStatement;

                var ifElseElement = new IfElse { Condition = _expressionTransformer.Transform(ifElse.Condition, context, block) };

                var trueBlock = new InlineBlock();
                Transform(ifElse.TrueStatement, context, trueBlock);
                ifElseElement.True = trueBlock;

                if (ifElse.FalseStatement != Statement.Null)
                {
                    var falseBlock = new InlineBlock();
                    Transform(ifElse.FalseStatement, context, falseBlock);
                    ifElseElement.Else = falseBlock;
                }

                block.Body.Add(ifElseElement);
            }
            else if (statement is BlockStatement)
            {
                var blockStatement = statement as BlockStatement;

                var statementBlock = new InlineBlock();

                foreach (var stmt in blockStatement.Statements)
                {
                    Transform(stmt, context, statementBlock);
                }

                block.Body.Add(statementBlock);
            }
            else if (statement is WhileStatement)
            {
                var whileStatement = statement as WhileStatement;

                var whileElement = new While { Condition = _expressionTransformer.Transform(whileStatement.Condition, context, block) };

                var bodyBlock = new InlineBlock();
                Transform(whileStatement.EmbeddedStatement, context, bodyBlock);
                whileElement.Body.Add(bodyBlock);

                block.Body.Add(whileElement);
            }
            else throw new NotSupportedException("Statements of type " + statement.GetType() + " are not supported to be transformed to VHDL.");
        }
    }
}
