using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.SubTranspilers
{
    public class MethodTranspiler
    {
        private readonly TypeConverter _typeConverter;
        private readonly StatementTranspiler _statementTranspiler;


        public MethodTranspiler()
            : this(new TypeConverter(), new StatementTranspiler())
        {
        }

        public MethodTranspiler(TypeConverter typeConverter, StatementTranspiler statementTranspiler)
        {
            _typeConverter = typeConverter;
            _statementTranspiler = statementTranspiler;
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
            var returnType = _typeConverter.Convert(method.ReturnType);
            var isVoid = returnType.Name == "void";
            ProcedureParameter outputParam = null;
            if (!isVoid)
            {
                // Since this way there's a dot in the output var's name, it can't clash with normal variables.
                outputParam = new ProcedureParameter { ObjectType = ObjectType.Variable, DataType = returnType, ParameterType = ProcedureParameterType.Out, Name = "output.var" };

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
                    var type = _typeConverter.Convert(parameter.Type);
                    var procedureParam = new ProcedureParameter { ObjectType = ObjectType.Variable, DataType = type, ParameterType = ProcedureParameterType.In, Name = parameter.Name + ".param" };

                    // Since In params can't be assigned to but C# method arguments can we copy the In params to local variables
                    procedure.Declarations.Add(new Variable { DataType = type, Name = parameter.Name });
                    procedure.Body.Add(new Raw(parameter.Name.ToVhdlId() + " := " + procedureParam.Name.ToVhdlId() + ";"));

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
            var bodyContext = new SubTranspilerContext
            {
                TranspilingContext = context,
                Scope = new SubTranspilerScope
                {
                    Node = method,
                    SubProgram = procedure
                }
            };
            foreach (var statement in method.Body.Statements)
            {
                _statementTranspiler.Transpile(statement, bodyContext, procedure);
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
