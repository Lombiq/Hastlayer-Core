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
using Hast.Communication.Extensibility;

namespace Hast.Communication
{
    public class MemberInvocationHandlerFactory : IMemberInvocationHandlerFactory
    {
        private readonly IWorkContextAccessor _wca;


        public MemberInvocationHandlerFactory(IWorkContextAccessor wca)
        {
            _wca = wca;
        }


        public MemberInvocationHandler CreateMemberInvocationHandler(IHardwareRepresentation hardwareRepresentation, object target)
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
                            var memberId = hardwareRepresentation.HardwareDescription.LookupMemberId(memberFullName);
                            // Need the wrapping Task to handle the async code.
                            var task = Task.Run(async () =>
                                {
                                    invocationContext.ExecutionInformation = await workContext
                                        .Resolve<ICommunicationService>()
                                        .Execute(memory, memberId);
                                });
                            task.Wait();
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
