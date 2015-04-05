using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Hast.Common.Models;
using Hast.Communication.Events;
using Orchard;
using Hast.Common.Extensions;

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
                        var context = new MethodInvocationContext
                        {
                            Invocation = invocation,
                            MethodFullName = invocation.Method.GetFullName(),
                            HardwareRepresentation = hardwareRepresentation
                        };


                        var hardwareMembers = hardwareRepresentation.HardwareDescription.HardwareMembers;
                        var memberNameAlternates = new HashSet<string>(hardwareMembers.SelectMany(member => member.GetMethodNameAlternates()));
                        if (!hardwareMembers.Contains(context.MethodFullName) && !memberNameAlternates.Contains(context.MethodFullName))
                        {
                            context.CancelHardwareInvocation = true;
                        }

                        workContext.Resolve<IMethodInvocationEventHandler>().MethodInvoked(context);

                        if (context.CancelHardwareInvocation) return false;

                        // Implement FPGA communication, data transformation here.
                        // Set the return value as invocation.ReturnValue = ...

                        return true;
                    }
                };
        }


        private class MethodInvocationContext : IMethodInvocationContext
        {
            public bool CancelHardwareInvocation { get; set; }
            public IInvocation Invocation { get; set; }
            public string MethodFullName { get; set; }
            public IHardwareRepresentation HardwareRepresentation { get; set; }
        }
    }
}
