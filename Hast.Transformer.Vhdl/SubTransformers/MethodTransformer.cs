using System.Collections.Generic;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Orchard;
using System.Linq;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IMethodTransformer : IDependency
    {
        void Transform(MethodDeclaration method, IVhdlTransformationContext context);
    }


    public class MethodTransformer : IMethodTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly IStatementTransformer _statementTransformer;


        public MethodTransformer(
            ITypeConverter typeConverter,
            IStatementTransformer statementTransformer)
        {
            _typeConverter = typeConverter;
            _statementTransformer = statementTransformer;
        }


        public void Transform(MethodDeclaration method, IVhdlTransformationContext context)
        {
            var fullName = method.GetFullName();
            var stateMachine = new MethodStateMachine(fullName);


            // Handling when the method is an interface method, i.e. should be executable from the host computer.
            InterfaceMethodDefinition interfaceMethod = null;
            if (method.IsInterfaceMember())
            {
                interfaceMethod = new InterfaceMethodDefinition
                {
                    Name = stateMachine.Name,
                    StateMachine = stateMachine,
                    Method = method
                };
                context.InterfaceMethods.Add(interfaceMethod);
            }


            var parameters = new List<Variable>();

            // Handling return type.
            var returnType = _typeConverter.Convert(method.ReturnType);
            var isVoid = returnType.Name == "void";
            if (!isVoid)
            {
                parameters.Add(new Variable
                    {
                        Name = stateMachine.CreateReturnVariableName(),
                        DataType = returnType
                    });
            }


            // Handling in/out method parameters.
            foreach (var parameter in method.Parameters.Where(p => !p.IsSimpleMemoryParameter()))
            {
                parameters.Add(new Variable
                    {
                        DataType = _typeConverter.Convert(parameter.Type),
                        Name = stateMachine.CreatePrefixedVariableName(parameter.Name)
                    });
            }

            stateMachine.Parameters = parameters;

            // Adding opening state and its block.
            var openingBlock = new InlineBlock();
            stateMachine.AddState(openingBlock);

            // Processing method body.
            var bodyContext = new SubTransformerContext
            {
                TransformationContext = context,
                Scope = new SubTransformerScope
                {
                    Method = method,
                    StateMachine = stateMachine,
                    CurrentBlock = new CurrentBlock(openingBlock)
                }
            };

            var lastStatementIsReturn = false;
            foreach (var statement in method.Body.Statements)
            {
                _statementTransformer.Transform(statement, bodyContext);
                lastStatementIsReturn = statement is ReturnStatement;
            }

            // If the last statement was a return statement then there is already a state change to the final state
            // added.
            if (!lastStatementIsReturn)
            {
                bodyContext.Scope.CurrentBlock.Add(stateMachine.ChangeToFinalState());
            }


            context.Module.Architecture.Declarations.Add(stateMachine.BuildDeclarations());
            context.Module.Architecture.Add(stateMachine.BuildBody());
        }
    }
}
