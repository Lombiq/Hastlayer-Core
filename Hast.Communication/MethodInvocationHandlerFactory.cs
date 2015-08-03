using Castle.DynamicProxy;
using Hast.Common.Extensibility.Pipeline;
using Hast.Common.Extensions;
using Hast.Common.Models;
using Hast.Communication.Extensibility.Events;
using Hast.Communication.Extensibility.Pipeline;
using Hast.Communication.Services;
using Hast.Transformer.SimpleMemory;
using Orchard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                        var context = new MethodInvocationPipelineStepContext
                        {
                            Invocation = invocation,
                            MethodFullName = invocation.Method.GetFullName(),
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
                            if (!hardwareMembers.Contains(context.MethodFullName) && !memberNameAlternates.Contains(context.MethodFullName))
                            {
                                context.HardwareInvocationIsCancelled = true;
                            } 
                        }

                        if (context.HardwareInvocationIsCancelled) return false;

                        // Implement FPGA communication, data transformation here.

                        
                        var memory = (SimpleMemory)invocation.Arguments.SingleOrDefault(argument => argument is SimpleMemory);
                        if (memory != null)
                        {
                            try
                            {
                                var task = Task.Run(async () => { await workContext.Resolve<ICommunicationService>().Execute(memory, 0); });
                                task.Wait();
                            }
                            catch (Exception e)
                            {
                                //TODO: What to do, if something went wrong with the serial port communication.
                                Debug.WriteLine(e.Message);
                            }
                            
                        }
                        // Debug.WriteLine("Execution completed...");
                        invocation.ReturnValue = memory.Memory;
                        // Set the return value as invocation.ReturnValue = ...

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
