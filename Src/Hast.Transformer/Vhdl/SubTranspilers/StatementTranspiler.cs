using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.Transformer.Vhdl.SubTranspilers
{
    public class StatementTranspiler
    {
        private readonly TypeConverter _typeConverter;
        private readonly ExpressionTranspiler _expressionTranspiler;


        public StatementTranspiler()
            : this(new TypeConverter(), new ExpressionTranspiler())
        {
        }

        public StatementTranspiler(TypeConverter typeConverter, ExpressionTranspiler expressionTranspiler)
        {
            _typeConverter = typeConverter;
            _expressionTranspiler = expressionTranspiler;
        }

        public void Transpile(Statement statement, SubTranspilerContext context, IBlockElement block)
        {
            var subProgram = context.Scope.SubProgram;

            if (statement is VariableDeclarationStatement)
            {
                var variableStatement = statement as VariableDeclarationStatement;

                subProgram.Declarations.Add(new Variable
                {
                    Name = string.Join(", ", variableStatement.Variables.Select(v => v.Name)),
                    DataType = _typeConverter.Convert(variableStatement.Type)
                });
            }
            else if (statement is ExpressionStatement)
            {
                var expressionStatement = statement as ExpressionStatement;

                block.Body.Add(new Terminated(_expressionTranspiler.Transpile(expressionStatement.Expression, context, block)));
            }
            else if (statement is ReturnStatement)
            {
                var returnStatement = statement as ReturnStatement;

                if (context.Scope.Node is MethodDeclaration)
                {
                    if (_typeConverter.Convert(((MethodDeclaration)context.Scope.Node).ReturnType).Name == "void") block.Body.Add(new Raw("return;"));
                    else if (subProgram is Procedure)
                    {
                        var procedure = subProgram as Procedure;

                        var outputParam = procedure.Parameters.Where(param => param.ParameterType == ProcedureParameterType.Out).Single();

                        var source = outputParam.Name.ToVhdlId() +
                                     (outputParam.ObjectType == ObjectType.Variable ? " := " : " <= ") +
                                     _expressionTranspiler.Transpile(returnStatement.Expression, context, block).ToVhdl() +
                                     "; return;";

                        procedure.Body.Add(new Raw(source));
                    }
                }
            }
            else if (statement is IfElseStatement)
            {
                var ifElse = statement as IfElseStatement;

                var ifElseElement = new IfElse { Condition = _expressionTranspiler.Transpile(ifElse.Condition, context, block).ToVhdl() };

                var trueBlock = new InlineBlock();
                Transpile(ifElse.TrueStatement, context, trueBlock);
                ifElseElement.TrueElements.Add(trueBlock);

                if (ifElse.FalseStatement != Statement.Null)
                {
                    var falseBlock = new InlineBlock();
                    Transpile(ifElse.TrueStatement, context, falseBlock);
                    ifElseElement.ElseElements.Add(falseBlock);
                }

                block.Body.Add(ifElseElement);
            }
            else if (statement is BlockStatement)
            {
                var blockStatement = statement as BlockStatement;

                var statementBlock = new InlineBlock();

                foreach (var stmt in blockStatement.Statements)
                {
                    Transpile(stmt, context, statementBlock);
                }

                block.Body.Add(statementBlock);
            }
            else if (statement is WhileStatement)
            {
                var whileStatement = statement as WhileStatement;

                var whileElement = new While { Condition = _expressionTranspiler.Transpile(whileStatement.Condition, context, block).ToVhdl() };

                var bodyBlock = new InlineBlock();
                Transpile(whileStatement.EmbeddedStatement, context, bodyBlock);
                whileElement.Body.Add(bodyBlock);

                block.Body.Add(whileElement);
            }
            else throw new NotSupportedException("Statements of type " + statement.GetType() + " are not supported.");
        }
    }
}
