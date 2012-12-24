using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation;
using VhdlBuilder.Representation.Declaration;
using VhdlBuilder;

namespace HastTranspiler.Vhdl.SubTranspilers
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

        public void Transpile(Statement statement, MethodBodyContext context)
        {
            var procedure = context.Scope.Procedure;

            if (statement is VariableDeclarationStatement)
            {
                var variableStatement = statement as VariableDeclarationStatement;

                procedure.Declarations.Add(new Variable
                {
                    Name = string.Join(", ", variableStatement.Variables.Select(v => v.Name)),
                    DataType = _typeConverter.Convert(variableStatement.Type)
                });
            }
            else if (statement is ExpressionStatement)
            {
                var expressionStatement = statement as ExpressionStatement;

                procedure.Body.Add(new Terminated(_expressionTranspiler.Transpile(expressionStatement.Expression, context)));
            }
            else if (statement is ReturnStatement)
            {
                var returnStatement = statement as ReturnStatement;

                if (_typeConverter.Convert(context.Scope.Method.ReturnType).Name == "void") procedure.Body.Add(new Raw("return;"));
                else
                {
                    var outputParam = procedure.Parameters.Where(param => param.ParameterType == ProcedureParameterType.Out).Single();

                    var source = outputParam.Name.ToVhdlId() +
                                 (outputParam.ObjectType == ObjectType.Variable ? " := " : " <= ") +
                                 _expressionTranspiler.Transpile(returnStatement.Expression, context).ToVhdl() +
                                 "; return;";

                    procedure.Body.Add(new Raw(source));
                }
            }
        }
    }
}
