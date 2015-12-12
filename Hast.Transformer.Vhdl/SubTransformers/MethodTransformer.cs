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
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Helpers;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IMethodTransformer : IDependency
    {
        Task Transform(MethodDeclaration method, IVhdlTransformationContext context);
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


        public async Task Transform(MethodDeclaration method, IVhdlTransformationContext context)
        {
            var stateMachineComputingTasks = new List<Task<StateMachineResult>>();

            for (int i = 0; i < context.GetTransformerConfiguration().MaxCallStackDepth; i++)
            {
                stateMachineComputingTasks.Add(BuildStateMachineFromMethod(method, context, i));
            }

            var stateMachineResults = await Task.WhenAll(stateMachineComputingTasks);

            // Handling when the method is an interface method, i.e. should be executable from the host computer.
            InterfaceMethodDefinition interfaceMethod = null;
            if (method.IsInterfaceMember())
            {
                interfaceMethod = new InterfaceMethodDefinition
                {
                    Name = stateMachineResults[0].StateMachine.Name,
                    StateMachine = stateMachineResults[0].StateMachine,
                    Method = method
                };
                context.InterfaceMethods.Add(interfaceMethod);
            }

            foreach (var result in stateMachineResults)
            {
                context.Module.Architecture.Declarations.Add(result.Declarations);
                context.Module.Architecture.Add(result.Body);
            }
        }


        private Task<StateMachineResult> BuildStateMachineFromMethod(
            MethodDeclaration method, 
            IVhdlTransformationContext context,
            int stateMachineIndex)
        {
            return Task.Run(() =>
                {
                    var stateMachine = new MethodStateMachine(MethodStateMachineNameFactory.CreateStateMachineName(method.GetFullName(), stateMachineIndex));


                    // Handling return type.
                    var returnType = _typeConverter.Convert(method.ReturnType);
                    var isVoid = returnType.Name == "void";
                    if (!isVoid)
                    {
                        stateMachine.Parameters.Add(new Variable
                        {
                            Name = stateMachine.CreateReturnVariableName(),
                            DataType = returnType
                        });
                    }

                    // Handling in/out method parameters.
                    foreach (var parameter in method.Parameters.Where(p => !p.IsSimpleMemoryParameter()))
                    {
                        stateMachine.Parameters.Add(new Variable
                        {
                            DataType = _typeConverter.Convert(parameter.Type),
                            Name = stateMachine.CreatePrefixedVariableName(parameter.Name)
                        });
                    }


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


                    // We need to return the declarations and body here too so their computation can be parallelised too.
                    // Otherwise we'd add them directly to context.Module.Architecture but that would need that collection to
                    // be thread-safe.
                    return new StateMachineResult
                    {
                        StateMachine = stateMachine,
                        Declarations = stateMachine.BuildDeclarations(),
                        Body = stateMachine.BuildBody()
                    };
                });
        }


        private class StateMachineResult
        {
            public IMethodStateMachine StateMachine { get; set; }
            public IVhdlElement Declarations { get; set; }
            public IVhdlElement Body { get; set; }
        }
    }
}
