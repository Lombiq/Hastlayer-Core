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
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.StateMachineGeneration;

namespace Hast.Transformer.Vhdl.SubTransformers
{
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
            var stateMachineCount = context.GetTransformerConfiguration().MaxCallStackDepth;
            var stateMachineResults = new StateMachineResult[stateMachineCount];

            // Not much use to parallelize computation unless there are a lot of state machines to create or the method
            // is very complex. We'll need to examine when to parallelize here and determine it in runtime.
            if (stateMachineCount > 50)
            {
                var stateMachineComputingTasks = new List<Task<StateMachineResult>>();

                for (int i = 0; i < stateMachineCount; i++)
                {
                    stateMachineComputingTasks.Add(Task.Run(() => BuildStateMachineFromMethod(method, context, i)));
                }

                stateMachineResults = await Task.WhenAll(stateMachineComputingTasks);
            }
            else
            {
                for (int i = 0; i < stateMachineCount; i++)
                {
                    stateMachineResults[i] = BuildStateMachineFromMethod(method, context, i);
                }
            }

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

            // If we wanted to parallelize individual method transforms then these calls should be made thread-safe.
            // One option would be to externalize adding the declarations and body just as it's not part of 
            // BuildStateMachineFromMethod. A better would be to use locking somehow to synchronize access to the two
            // collections (locking shouldn't hurt performance too much in this case).
            foreach (var result in stateMachineResults)
            {
                context.Module.Architecture.Declarations.Add(result.Declarations);
                context.Module.Architecture.Add(result.Body);
            }
        }


        private StateMachineResult BuildStateMachineFromMethod(
            MethodDeclaration method,
            IVhdlTransformationContext context,
            int stateMachineIndex)
        {
            var stateMachine = new MemberStateMachine(MemberStateMachineNameFactory.CreateStateMachineName(method.GetFullName(), stateMachineIndex));


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
            var openingStateIndex = stateMachine.AddState(openingBlock);

            // Processing method body.
            var bodyContext = new SubTransformerContext
            {
                TransformationContext = context,
                Scope = new SubTransformerScope
                {
                    Method = method,
                    StateMachine = stateMachine,
                    CurrentBlock = new CurrentBlock(stateMachine, openingBlock, openingStateIndex)
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


            // We need to return the declarations and body here too so their computation can be parallelized too.
            // Otherwise we'd add them directly to context.Module.Architecture but that would need that collection to
            // be thread-safe.
            return new StateMachineResult
            {
                StateMachine = stateMachine,
                Declarations = stateMachine.BuildDeclarations(),
                Body = stateMachine.BuildBody()
            };
        }


        private class StateMachineResult
        {
            public IMemberStateMachine StateMachine { get; set; }
            public IVhdlElement Declarations { get; set; }
            public IVhdlElement Body { get; set; }
        }
    }
}
