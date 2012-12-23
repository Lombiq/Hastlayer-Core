using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation;
using VhdlBuilder;

namespace HastTranspiler.Vhdl.SubTranspilers
{
    public class MethodTranspiler
    {
        public TypeConverter TypeConverter { get; set; }
        public ExpressionTranspiler ExpressionTranspiler { get; set; }


        public MethodTranspiler()
        {
            TypeConverter = new TypeConverter();
            ExpressionTranspiler = new ExpressionTranspiler();
        }


        // TODO: which Transpile() works better?
        public IVhdlElement Transpile(MethodDeclaration method)
        {
            throw new NotImplementedException();
        }

        public void Transpile(MethodDeclaration method, TranspilingContext context)
        {
            var fullName = NameUtility.GetFullName(method);
            var interfaceMethod = CreateInterfaceMethodIfPublic(method, fullName);
            if (interfaceMethod != null) context.InterfaceMethods.Add(interfaceMethod);

            var procedure = new Procedure { Name = fullName };

            var parameters = new List<ProcedureParameter>();
            var returnType = TypeConverter.Convert(method.ReturnType);
            ProcedureParameter outputParam = null;
            if (returnType.Name != "void")
            {
                outputParam = new ProcedureParameter { DataType = returnType, ParameterType = ProcedureParameterType.Out, Name = "output" };

                if (interfaceMethod != null)
                {
                    outputParam.ObjectType = ObjectType.Signal;
                    outputParam.Name = fullName + ".output";
                }

                parameters.Add(outputParam);
            }

            if (method.Parameters.Count != 0)
            {
                foreach (var parameter in method.Parameters)
                {
                    var type = TypeConverter.Convert(parameter.Type);
                    var procedureParam = new ProcedureParameter { DataType = type, ParameterType = ProcedureParameterType.In, Name = parameter.Name };
                    if (interfaceMethod != null)
                    {
                        procedureParam.ObjectType = ObjectType.Signal;
                        procedureParam.Name = NameUtility.GetFullName(parameter);
                        procedure.Declarations.Add(new DataObject
                        {
                            Type = ObjectType.Variable,
                            DataType = type,
                            Name = parameter.Name,
                            Value = new Value { DataType = DataTypes.Identifier, Content = procedureParam.Name.ToVhdlId() }
                        });
                    }
                    parameters.Add(procedureParam);
                }
            }
            procedure.Parameters = parameters;

            foreach (var statement in method.Body.Statements)
            {
                if (statement is VariableDeclarationStatement)
                {
                    var variableStatement = statement as VariableDeclarationStatement;

                    procedure.Declarations.Add(new DataObject
                    {
                        Type = ObjectType.Variable,
                        Name = string.Join(", ", variableStatement.Variables.Select(v => v.Name)),
                        DataType = TypeConverter.Convert(variableStatement.Type)
                    });
                }
                else if (statement is ExpressionStatement)
                {
                    var expressionStatement = statement as ExpressionStatement;

                    procedure.Body.Add(new Terminated(ExpressionTranspiler.Transpile(expressionStatement.Expression)));
                }
                else if (statement is ReturnStatement)
                {
                    var returnStatement = statement as ReturnStatement;
                    procedure.Body.Add(
                        new Raw(
                            outputParam.Name.ToVhdlId() + 
                                (outputParam.ObjectType == ObjectType.Variable ? " := " : " <= ") + 
                                ExpressionTranspiler.Transpile(returnStatement.Expression).ToVhdl() + 
                                ";"
                        ));
                }
            }

            context.Module.Architecture.Declarations.Add(procedure);
        }

        private InterfaceMethodDefinition CreateInterfaceMethodIfPublic(MethodDeclaration method, string fullName)
        {
            if (method.Modifiers != (Modifiers.Public | Modifiers.Virtual) || !(method.Parent is TypeDeclaration)) return null;

            var parent = method.Parent as TypeDeclaration;

            if (parent.ClassType != ClassType.Class || parent.Modifiers != Modifiers.Public) return null;

            var ports = new List<Port>();

            var returnType = TypeConverter.Convert(method.ReturnType);
            if (returnType.Name != "void") ports.Add(new Port { DataType = returnType, Mode = PortMode.Out, Name = fullName + ".output" });

            if (method.Parameters.Count != 0)
            {
                foreach (var parameter in method.Parameters)
                {
                    ports.Add(new Port { DataType = TypeConverter.Convert(parameter.Type), Mode = PortMode.In, Name = NameUtility.GetFullName(parameter) });
                }
            }

            return new InterfaceMethodDefinition { Name = fullName, Ports = ports };
        }
    }
}
