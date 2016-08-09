using Castle.DynamicProxy;
using Hast.Common.Extensibility.Pipeline;
using Hast.Common.Extensions;
using Hast.Common.Models;
using Hast.Communication.Extensibility.Events;
using Hast.Communication.Extensibility.Pipeline;
using Hast.Communication.Services;
using Hast.Transformer.SimpleMemory;
using Orchard;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System;
using Hast.Common.Configuration;
using Hast.Communication.Exceptions;

namespace Hast.Communication
{
    public class MemberInvocationHandlerFactory : IMemberInvocationHandlerFactory
    {
        private readonly IWorkContextAccessor _wca;


        public MemberInvocationHandlerFactory(IWorkContextAccessor wca)
        {
            _wca = wca;
        }


        public MemberInvocationHandler CreateMemberInvocationHandler(
            IHardwareRepresentation hardwareRepresentation, 
            object target,
            IProxyGenerationConfiguration configuration)
        {
            return invocation =>
                {
                    using (var workContext = _wca.CreateWorkContextScope())
                    {
                        var methodAsynchronicity = GetMethodAsynchronicity(invocation);

                        if (methodAsynchronicity == MethodAsynchronicity.AsyncFunction)
                        {
                            throw new NotSupportedException("Only async methods that return a Task, not Task<T>, are supported.");
                        }

                        // Although it says Method it can also be a property.
                        var memberFullName = invocation.Method.GetFullName();

                        var invocationContext = new MemberInvocationContext
                        {
                            Invocation = invocation,
                            MemberFullName = memberFullName,
                            HardwareRepresentation = hardwareRepresentation
                        };

                        var eventHandler = workContext.Resolve<IMemberInvocationEventHandler>();
                        eventHandler.MemberInvoking(invocationContext);

                        workContext.Resolve<IEnumerable<IMemberInvocationPipelineStep>>().InvokePipelineSteps(step =>
                            {
                                invocationContext.HardwareExecutionIsCancelled = step.CanContinueHardwareExecution(invocationContext);
                            });

                        if (!invocationContext.HardwareExecutionIsCancelled)
                        {
                            var hardwareMembers = hardwareRepresentation.HardwareDescription.HardwareMembers;
                            var memberNameAlternates = new HashSet<string>(hardwareMembers.SelectMany(member => member.GetMemberNameAlternates()));
                            if (!hardwareMembers.Contains(memberFullName) && !memberNameAlternates.Contains(memberFullName))
                            {
                                invocationContext.HardwareExecutionIsCancelled = true;
                            } 
                        }

                        if (invocationContext.HardwareExecutionIsCancelled) return false;
                  
                        var memory = (SimpleMemory)invocation.Arguments.SingleOrDefault(argument => argument is SimpleMemory);
                        if (memory != null)
                        {
                            SimpleMemory softMemory = null;

                            if (configuration.ValidateHardwareResults)
                            {
                                softMemory = new SimpleMemory(memory.CellCount);
                                memory.Memory.CopyTo(softMemory.Memory, 0);
                            }

                            var memberId = hardwareRepresentation.HardwareDescription.LookupMemberId(memberFullName);
                            // Need the wrapping Task to handle the async code.
                            var task = Task.Run(async () =>
                                {
                                    invocationContext.ExecutionInformation = await workContext
                                        .Resolve<ICommunicationServiceSelector>()
                                        .GetCommunicationService(configuration.CommunicationChannelName)
                                        .Execute(memory, memberId);
                                });
                            task.Wait();

                            if (configuration.ValidateHardwareResults)
                            {
                                var memoryArgumentIndex = invocation.Arguments
                                    .Select((argument, index) => new { Argument = argument, Index = index })
                                    .Single(argument => argument.Argument is SimpleMemory)
                                    .Index;
                                invocation.SetArgumentValue(memoryArgumentIndex, softMemory);

                                invocation.Proceed();

                                var mismatches = new List<HardwareExecutionResultMismatchException.Mismatch>();
                                for (int i = 0; i < memory.CellCount; i++)
                                {
                                    var hardwareBytes = memory.Read4Bytes(i);
                                    var softwareBytes = softMemory.Read4Bytes(i);
                                    if (!hardwareBytes.SequenceEqual(softwareBytes))
                                    {
                                        mismatches.Add(new HardwareExecutionResultMismatchException.Mismatch(
                                            i, hardwareBytes, softwareBytes));
                                    }
                                }

                                if (mismatches.Any())
                                {
                                    throw new HardwareExecutionResultMismatchException(mismatches);
                                }
                            }
                        }
                        else
                        {
                            throw new NotSupportedException("Only SimpleMemory-using implementations are supported for hardware execution.");
                        }

                        eventHandler.MemberExecutedOnHardware(invocationContext);

                        if (methodAsynchronicity == MethodAsynchronicity.AsyncAction)
                        {
                            invocation.ReturnValue = Task.FromResult(true);
                        }

                        return true;
                    }
                };
        }


        // Code taken from http://stackoverflow.com/a/28374134/220230
        private static MethodAsynchronicity GetMethodAsynchronicity(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;

            if (returnType == typeof(Task)) return MethodAsynchronicity.AsyncAction;

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                return MethodAsynchronicity.AsyncFunction;

            return MethodAsynchronicity.Synchronous;
        }


        private enum MethodAsynchronicity
        {
            Synchronous,
            AsyncAction,
            AsyncFunction
        }

        private class MemberInvocationContext : IMemberInvocationPipelineStepContext, IMemberHardwareExecutionContext
        {
            public bool HardwareExecutionIsCancelled { get; set; }
            public IInvocation Invocation { get; set; }
            public string MemberFullName { get; set; }
            public IHardwareRepresentation HardwareRepresentation { get; set; }
            public IHardwareExecutionInformation ExecutionInformation { get; set; }
        }
    }
}
