using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation;

namespace HastTranspiler.Vhdl
{
    public static class MethodTranspiler
    {
        public static Entity CreateEntityIfPublic(MethodDeclaration method)
        {
            if (method.Modifiers != Modifiers.Public || !(method.Parent is TypeDeclaration)) return null;

            var parent = method.Parent as TypeDeclaration;

            if (parent.ClassType != ClassType.Class || parent.Modifiers != Modifiers.Public) return null;

            var ports = new List<Port>();

            var returnType = TypeConverter.Convert(method.ReturnType);
            if (returnType.Name != "void") ports.Add(new Port { DataType = returnType, Mode = PortMode.Out, Name = "output" });

            if (method.Parameters.Count != 0)
            {
                foreach (var parameter in method.Parameters)
                {
                    ports.Add(new Port { DataType = TypeConverter.Convert(parameter.Type), Mode = PortMode.In, Name = parameter.Name });
                }
            }

            return new Entity { Name = parent.Name + "_" + method.Name, Ports = ports };
        }

        public static IVhdlElement Transpile(MethodDeclaration method)
        {
            var process = new Process();

            foreach (var statement in method.Body.Statements)
            {
                if (statement is VariableDeclarationStatement)
                {
                    var variableStatement = statement as VariableDeclarationStatement;
                    
                    process.Declarations.Add(new Variable
                    {
                        Name = String.Join(", ", variableStatement.Variables.Select(v => v.Name)),
                        DataType = TypeConverter.Convert(variableStatement.Type)
                    });
                }
                else if (statement is ExpressionStatement)
                {
                    var expressionStatement = statement as ExpressionStatement;

                    process.Body.Add(ExpressionTranspiler.Transpile(expressionStatement.Expression));
                }
                else if (statement is ReturnStatement)
                {
                    
                }
            }

            return process;
        }
    }
}
