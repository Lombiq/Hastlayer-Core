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

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class MethodTransformer
    {
        private readonly TypeConverter _typeConverter;
        private readonly StatementTransformer _statementTransformer;


        public MethodTransformer()
            : this(new TypeConverter(), new StatementTransformer())
        {
        }

        public MethodTransformer(TypeConverter typeConverter, StatementTransformer statementTransformer)
        {
            _typeConverter = typeConverter;
            _statementTransformer = statementTransformer;
        }


        public void Transform(MethodDeclaration method, IVhdlTransformationContext context)
        {
            var fullName = method.GetFullName();
            var procedure = new Procedure { Name = fullName };

            // Handling when the method is an interface method, i.e. should be present in the interface of the VHDL module.
            InterfaceMethodDefinition interfaceMethod = null;
            if (method.Parent is TypeDeclaration &&
                    (method.Modifiers == (Modifiers.Public | Modifiers.Virtual) || // If it's a public virtual method.
                    IsInterfaceDeclaredMethod(method, context)) // Or a public method that implements an interface.
               )
            {
                var parent = (TypeDeclaration)method.Parent;
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
            var bodyContext = new SubTransformerContext
            {
                TransformationContext = context,
                Scope = new SubTransformerScope
                {
                    Node = method,
                    SubProgram = procedure
                }
            };
            foreach (var statement in method.Body.Statements)
            {
                _statementTransformer.Transform(statement, bodyContext, procedure);
            }


            context.Module.Architecture.Declarations.Add(procedure);
        }


        private static bool IsInterfaceDeclaredMethod(MethodDeclaration method, IVhdlTransformationContext context)
        {
            // Is this an explicitly implemented interface method?
            if (method.Modifiers == Modifiers.None &&
                method.NameToken.NextSibling != null &&
                method.NameToken.NextSibling.NodeType == NodeType.TypeReference)
            {
                return true;
            }

            // Otherwise if it's not public it can't be a method declared in an interface (public virtuals are checked separately).
            if (method.Modifiers != Modifiers.Public) return false;

            // Searching for an implemented interface with the same method.
            var parent = (TypeDeclaration)method.Parent;
            foreach (var baseType in parent.BaseTypes) // BaseTypes are flattened, so interface inheritance is taken into account.
            {
                if (baseType.NodeType == NodeType.TypeReference)
                {
                    // baseType is a TypeReference but we need the corresponding TypeDeclaration to check for the methods.
                    var baseTypeDeclaration = context.LookupDeclaration(baseType);

                    if (baseTypeDeclaration.ClassType == ClassType.Interface)
                    {
                        if (baseTypeDeclaration.Members.Any(member =>
                            {
                                if (member.Name == method.Name && member.EntityType == ICSharpCode.NRefactory.TypeSystem.EntityType.Method)
                                {
                                    var interfaceMethod = (MethodDeclaration)member;
                                    if (interfaceMethod.ReturnType.TypeEquals(method.ReturnType, context.LookupDeclaration) &&
                                        interfaceMethod.Parameters.Count == method.Parameters.Count)
                                    {
                                        foreach (var interfaceMethodParameter in interfaceMethod.Parameters)
                                        {
                                            if (!method.Parameters.Any(parameter => parameter.Type.TypeEquals(interfaceMethodParameter.Type, context.LookupDeclaration)))
                                            {
                                                return false;
                                            }
                                        }
                                        return true;
                                    }
                                }

                                return false;
                            }))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
