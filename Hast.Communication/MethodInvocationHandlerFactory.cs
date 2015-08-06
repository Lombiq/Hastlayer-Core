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

namespace Hast.Communication
{
    public class MethodInvocationHandlerFactory : IMethodInvocationHandlerFactory
    {
        private readonly IWorkContextAccessor _wca;


        public MethodInvocationHandlerFactory(IWorkContextAccessor wca)
        {
            _wca = wca;
        }


        public MethodInvocationHandler CreateMethodInvocationHandler(IHardwareRepresentation hardwareRepresentation, object target)
        {
            return invocation =>
                {
                    using (var workContext = _wca.CreateWorkContextScope())
                    {
                        var methodFullName = invocation.Method.GetFullName();

                        var context = new MethodInvocationPipelineStepContext
                        {
                            Invocation = invocation,
                            MethodFullName = methodFullName,
                            HardwareRepresentation = hardwareRepresentation
                        };

                        var eventHandler = workContext.Resolve<IMethodInvocationEventHandler>();
                        eventHandler.MethodInvoking(context);

                        workContext.Resolve<IEnumerable<IMethodInvocationPipelineStep>>().InvokePipelineSteps(step =>
                            {
                                context.HardwareInvocationIsCancelled = step.CanContinueHardwareInvokation(context);
                            });

                        if (!context.HardwareInvocationIsCancelled)
                        {
                            var hardwareMembers = hardwareRepresentation.HardwareDescription.HardwareMembers;
                            var memberNameAlternates = new HashSet<string>(hardwareMembers.SelectMany(member => member.GetMethodNameAlternates()));
                            if (!hardwareMembers.Contains(methodFullName) && !memberNameAlternates.Contains(methodFullName))
                            {
                                context.HardwareInvocationIsCancelled = true;
                            } 
                        }

                        if (context.HardwareInvocationIsCancelled) return false;
                  
                        var memory = (SimpleMemory)invocation.Arguments.SingleOrDefault(argument => argument is SimpleMemory);
                        if (memory != null)
                        {
                            var methodId = hardwareRepresentation.HardwareDescription.LookupMethodId(methodFullName);
                            // The task here is needed because the code executed on the FPGA board doesn't return, we have to wait for it.
                            // The Execute method is executed in separate thread.
                            var task = Task.Run(async () => { await workContext.Resolve<ICommunicationService>().Execute(memory, methodId); });
                            task.Wait();                   
                        }

                        eventHandler.MethodInvokedOnHardware(context);

                        return true;
                    }
                };
        }


        private class MethodInvocationPipelineStepContext : IMethodInvocationPipelineStepContext
        {
            public bool HardwareInvocationIsCancelled { get; set; }
            public IInvocation Invocation { get; set; }
            public string MethodFullName { get; set; }
            public IHardwareRepresentation HardwareRepresentation { get; set; }
        }
    }
}
