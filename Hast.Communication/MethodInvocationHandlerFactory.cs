using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Hast.Common.Models;
using Orchard;
using Hast.Common.Extensions;
using Hast.Communication.Extensibility.Events;
using Hast.Communication.Extensibility;
using Hast.Communication.Extensibility.Pipeline;
using Hast.Common.Extensibility.Pipeline;
using Hast.Transformer.SimpleMemory;

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

                        Communication com = new Communication();
                        com.Start(); // Initialize the communication
                        var memory = (SimpleMemory)invocation.Arguments.SingleOrDefault(argument => argument is SimpleMemory);
                        if (memory != null)
                        {
                            memory = com.Execute(memory);
                        }
                        
                        
                        
                       

                        

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
