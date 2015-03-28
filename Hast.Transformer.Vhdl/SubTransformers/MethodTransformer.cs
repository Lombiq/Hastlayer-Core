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
using Hast.Transformer.Vhdl.Models;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IMethodTransformer : IDependency
    {
        void Transform(MethodDeclaration method, IVhdlTransformationContext context);
    }


    public class MethodTransformer : IMethodTransformer
    {
        private readonly IMemberSuitabilityChecker _memberSuitabilityChecker;
        private readonly ITypeConverter _typeConverter;
        private readonly IStatementTransformer _statementTransformer;


        public MethodTransformer(
            IMemberSuitabilityChecker memberSuitabilityChecker,
            ITypeConverter typeConverter,
            IStatementTransformer statementTransformer)
        {
            _memberSuitabilityChecker = memberSuitabilityChecker;
            _typeConverter = typeConverter;
            _statementTransformer = statementTransformer;
        }


        public void Transform(MethodDeclaration method, IVhdlTransformationContext context)
        {
            var fullName = method.GetFullName();
            var procedure = new Procedure { Name = fullName };


            // Handling when the method is an interface method, i.e. should be present in the interface of the VHDL module.
            InterfaceMethodDefinition interfaceMethod = null;
            if (_memberSuitabilityChecker.IsSuitableInterfaceMember(method, context.TypeDeclarationLookupTable))
            {
                interfaceMethod = new InterfaceMethodDefinition { Name = procedure.Name, Procedure = procedure };
                context.InterfaceMethods.Add(interfaceMethod);
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
            var bodyContext = new SubTransformerContext
            {
                TransformationContext = context,
                Scope = new SubTransformerScope
                {
                    Method = method,
                    SubProgram = procedure
                }
            };
            foreach (var statement in method.Body.Statements)
            {
                _statementTransformer.Transform(statement, bodyContext, procedure);
            }


            context.Module.Architecture.Declarations.Add(procedure);
        }
    }
}
