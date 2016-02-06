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
using Hast.Transformer.Vhdl.ArchitectureComponentBuilding;
using Hast.Common.Configuration;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class MethodTransformer : IMethodTransformer
    {
        private readonly IMemberStateMachineFactory _memberStateMachineFactory;
        private readonly ITypeConverter _typeConverter;
        private readonly IStatementTransformer _statementTransformer;


        public MethodTransformer(
            IMemberStateMachineFactory memberStateMachineFactory,
            ITypeConverter typeConverter,
            IStatementTransformer statementTransformer)
        {
            _memberStateMachineFactory = memberStateMachineFactory;
            _typeConverter = typeConverter;
            _statementTransformer = statementTransformer;
        }


        public Task<IMemberTransformerResult> Transform(MethodDeclaration method, IVhdlTransformationContext context)
        {
            return Task.Run(async () =>
                {
                    var stateMachineCount = context
                        .GetTransformerConfiguration()
                        .GetMaxCallInstanceCountConfigurationForMember(method.GetSimpleName()).MaxCallInstanceCount;
                    var stateMachineResults = new IMemberStateMachineResult[stateMachineCount];

                    // Not much use to parallelize computation unless there are a lot of state machines to create or the 
                    // method is very complex. We'll need to examine when to parallelize here and determine it in runtime.
                    if (stateMachineCount > 50)
                    {
                        var stateMachineComputingTasks = new List<Task<IMemberStateMachineResult>>();

                        for (int i = 0; i < stateMachineCount; i++)
                        {
                            var task = new Task<IMemberStateMachineResult>(
                                index => BuildStateMachineFromMethod(method, context, (int)index),
                                i);
                            task.Start();
                            stateMachineComputingTasks.Add(task);
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

                    return (IMemberTransformerResult)new MemberTransformerResult
                    {
                        Member = method,
                        IsInterfaceMember = method.IsInterfaceMember(),
                        StateMachineResults = stateMachineResults
                    };
                });
        }


        private IMemberStateMachineResult BuildStateMachineFromMethod(
            MethodDeclaration method,
            IVhdlTransformationContext context,
            int stateMachineIndex)
        {
            var stateMachine = _memberStateMachineFactory
                .CreateStateMachine(MemberStateMachineNameHelper.CreateStateMachineName(method.GetFullName(), stateMachineIndex));


            // Handling return type.
            var returnType = _typeConverter.ConvertAstType(method.ReturnType);
            var isVoid = returnType.Name == "void";
            if (!isVoid)
            {
                stateMachine.GlobalVariables.Add(new Variable
                {
                    Name = stateMachine.CreateReturnVariableName(),
                    DataType = returnType
                });
            }

            // Handling in/out method parameters.
            foreach (var parameter in method.Parameters.Where(p => !p.IsSimpleMemoryParameter()))
            {
                stateMachine.GlobalVariables.Add(new Variable
                {
                    DataType = _typeConverter.ConvertAstType(parameter.Type),
                    Name = stateMachine.CreatePrefixedObjectName(parameter.Name)
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
            return new MemberStateMachineResult
            {
                StateMachine = stateMachine,
                Declarations = stateMachine.BuildDeclarations(),
                Body = stateMachine.BuildBody()
            };
        }
    }
}
