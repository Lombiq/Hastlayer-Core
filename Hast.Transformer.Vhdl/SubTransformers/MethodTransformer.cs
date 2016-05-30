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
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Common.Configuration;
using System;

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
                    if (method.Modifiers.HasFlag(Modifiers.Extern))
                    {
                        throw new InvalidOperationException(
                            "The method " + method.GetFullName() + 
                            " can't be transformed because it's extern. Only managed code can be transformed.");
                    }

                    var stateMachineCount = context
                        .GetTransformerConfiguration()
                        .GetMaxInvokationInstanceCountConfigurationForMember(method).MaxInvokationInstanceCount;
                    var stateMachineResults = new IArchitectureComponentResult[stateMachineCount];

                    // Not much use to parallelize computation unless there are a lot of state machines to create or the 
                    // method is very complex. We'll need to examine when to parallelize here and determine it in runtime.
                    if (stateMachineCount > 50)
                    {
                        var stateMachineComputingTasks = new List<Task<IArchitectureComponentResult>>();

                        for (int i = 0; i < stateMachineCount; i++)
                        {
                            var task = new Task<IArchitectureComponentResult>(
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
                        ArchitectureComponentResults = stateMachineResults
                    };
                });
        }


        private IArchitectureComponentResult BuildStateMachineFromMethod(
            MethodDeclaration method,
            IVhdlTransformationContext context,
            int stateMachineIndex)
        {
            var methodFullName = method.GetFullName();
            var stateMachine = _memberStateMachineFactory
                .CreateStateMachine(ArchitectureComponentNameHelper.CreateIndexedComponentName(methodFullName, stateMachineIndex));

            // Adding the opening state's block.
            var openingBlock = new InlineBlock();


            // Handling the return type.
            var returnType = _typeConverter.ConvertAstType(method.ReturnType);
            // If the return type is a Task then that means it's one of the supported simple TPL scenarios, corresponding
            // to void in VHDL.
            if (returnType == SpecialTypes.Task) returnType = KnownDataTypes.Void;
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
                // Since input parameters are assigned to from the outside but they could be attempted to be also assigned
                // to from the inside (since in .NET a method argument can also be assigned to from inside the method)
                // we need to have intermediary input variables, then copy their values to local variables.

                var parameterDataType = _typeConverter.ConvertAstType(parameter.Type);
                var parameterGlobalVariableName = stateMachine.CreateParameterVariableName(parameter.Name);
                var parameterLocalVariableName = stateMachine.CreatePrefixedObjectName(parameter.Name);

                stateMachine.GlobalVariables.Add(new ParameterVariable(methodFullName, parameter.Name)
                {
                    DataType = parameterDataType,
                    Name = parameterGlobalVariableName,
                    IsOwn = true
                });

                stateMachine.LocalVariables.Add(new Variable
                {
                    DataType = parameterDataType,
                    Name = parameterLocalVariableName
                });

                openingBlock.Add(new Assignment
                {
                    AssignTo = parameterLocalVariableName.ToVhdlVariableReference(),
                    Expression = parameterGlobalVariableName.ToVhdlVariableReference()
                });
            }


            // Processing method body.
            var bodyContext = new SubTransformerContext
            {
                TransformationContext = context,
                Scope = new SubTransformerScope
                {
                    Method = method,
                    StateMachine = stateMachine,
                    CurrentBlock = new CurrentBlock(stateMachine, openingBlock, stateMachine.AddState(openingBlock))
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
            return new ArchitectureComponentResult
            {
                ArchitectureComponent = stateMachine,
                Declarations = stateMachine.BuildDeclarations(),
                Body = stateMachine.BuildBody()
            };
        }
    }
}
