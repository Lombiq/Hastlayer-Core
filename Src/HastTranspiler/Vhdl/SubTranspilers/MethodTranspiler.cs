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
            var procedure = new Procedure { Name = fullName };

            // Handling when the method is an interface method
            InterfaceMethodDefinition interfaceMethod = null;
            if (method.Modifiers == (Modifiers.Public | Modifiers.Virtual) && method.Parent is TypeDeclaration)
            {
                var parent = method.Parent as TypeDeclaration;
                if (parent.ClassType == ClassType.Class && parent.Modifiers == Modifiers.Public)
                {
                    interfaceMethod = new InterfaceMethodDefinition { Name = fullName, Procedure = procedure };
                    context.InterfaceMethods.Add(interfaceMethod);
                }
            }

            var parameters = new List<ProcedureParameter>();

            // Handling return type
            var returnType = TypeConverter.Convert(method.ReturnType);
            ProcedureParameter outputParam = null;
            if (returnType.Name != "void")
            {
                outputParam = new ProcedureParameter { ObjectType = ObjectType.Variable, DataType = returnType, ParameterType = ProcedureParameterType.Out, Name = "output" };

                if (interfaceMethod != null)
                {
                    var outputPort = new Port { DataType = returnType, Mode = PortMode.Out, Name = fullName + ".output" };
                    interfaceMethod.ParameterMappings.Add(new ParameterMapping { Port = outputPort, Parameter = outputParam });
                    interfaceMethod.Ports.Add(outputPort);
                }

                parameters.Add(outputParam);
            }

            // Handling input parameters
            if (method.Parameters.Count != 0)
            {
                foreach (var parameter in method.Parameters)
                {
                    var type = TypeConverter.Convert(parameter.Type);
                    var procedureParam = new ProcedureParameter { ObjectType = ObjectType.Variable, DataType = type, ParameterType = ProcedureParameterType.In, Name = parameter.Name };

                    if (interfaceMethod != null)
                    {
                        var inputPort = new Port { DataType = type, Mode = PortMode.In, Name = fullName + "." + parameter.Name };
                        interfaceMethod.ParameterMappings.Add(new ParameterMapping { Port = inputPort, Parameter = procedureParam });
                        interfaceMethod.Ports.Add(inputPort);
                    }

                    parameters.Add(procedureParam);
                }
            }

            procedure.Parameters = parameters;


            // Processing method body
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

        public void ConnectSignalsToVariables(Procedure procedure)
        {
            //foreach (var parameter in procedure.Parameters.Where(param => param.ObjectType == ObjectType.Signal))
            //{
            //    if (parameter.ParameterType != ProcedureParameterType.Out)
            //    {
            //        procedure.Declarations.Add(new DataObject
            //        {
            //            Type = ObjectType.Variable,
            //            DataType = parameter.DataType,
            //            Name = parameter.Name,
            //            Value = new Value { DataType = DataTypes.Identifier, Content = parameter.Name.ToVhdlId() }
            //        });
            //    }
            //}
        }
    }
}
