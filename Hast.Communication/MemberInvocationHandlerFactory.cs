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

namespace Hast.Communication
{
    public class MemberInvocationHandlerFactory : IMemberInvocationHandlerFactory
    {
        private readonly IWorkContextAccessor _wca;


        public MemberInvocationHandlerFactory(IWorkContextAccessor wca)
        {
            _wca = wca;
        }


        public MemberInvocationHandler CreateMemberInvocationHandler(IHardwareRepresentation hardwareRepresentation, object target, IProxyGenerationConfiguration configuration)
        {
            return invocation =>
                {
                    using (var workContext = _wca.CreateWorkContextScope())
                    {
                        // Although it says Method it can also be a property.
                        var memberFullName = invocation.Method.GetFullName();

                        var invokationContext = new MemberInvocationContext
                        {
                            Invocation = invocation,
                            MemberFullName = memberFullName,
                            HardwareRepresentation = hardwareRepresentation
                        };

                        var eventHandler = workContext.Resolve<IMemberInvocationEventHandler>();
                        eventHandler.MemberInvoking(invokationContext);

                        workContext.Resolve<IEnumerable<IMemberInvocationPipelineStep>>().InvokePipelineSteps(step =>
                            {
                                invokationContext.HardwareExecutionIsCancelled = step.CanContinueHardwareExecution(invokationContext);
                            });

                        if (!invokationContext.HardwareExecutionIsCancelled)
                        {
                            var hardwareMembers = hardwareRepresentation.HardwareDescription.HardwareMembers;
                            var memberNameAlternates = new HashSet<string>(hardwareMembers.SelectMany(member => member.GetMemberNameAlternates()));
                            if (!hardwareMembers.Contains(memberFullName) && !memberNameAlternates.Contains(memberFullName))
                            {
                                invokationContext.HardwareExecutionIsCancelled = true;
                            }
                        }

                        if (invokationContext.HardwareExecutionIsCancelled) return false;
                  
                        var memory = (SimpleMemory)invocation.Arguments.SingleOrDefault(argument => argument is SimpleMemory);
                        if (memory != null)
                        {
                            var memberId = hardwareRepresentation.HardwareDescription.LookupMemberId(memberFullName) + 1;
                            // The task here is needed because the code executed on the FPGA board doesn't return, we have 
                            // to wait for it.
                            // The Execute method is executed on separate thread.
                            var task = Task.Run(async () =>
                                {
                                    invokationContext.ExecutionInformation = await workContext
                                        .Resolve<ICommunicationServiceSelector>()
                                        .GetCommunicationService(configuration.CommunicationChannelName)
                                        .Execute(memory, memberId);
                                });
                            task.Wait();
                        }
                        else
                        {
                            throw new InvalidOperationException("Only SimpleMemory-using implementations are supported for hardware execution.");
                        }

                        eventHandler.MemberExecutedOnHardware(invokationContext);

                        return true;
                    }
                };
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
