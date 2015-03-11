using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Hast.Communication.Events;
using Orchard;

namespace Hast.Communication
{
    public class MethodInvocationHandlerFactory : IMethodInvocationHandlerFactory
    {
        private readonly IWorkContextAccessor _wca;


        public MethodInvocationHandlerFactory(IWorkContextAccessor wca)
        {
            _wca = wca;
        }


        public MethodInvocationHandler CreateMethodInvocationHandler(object target)
        {
            return invocation =>
                {
                    using (var workContext = _wca.CreateWorkContextScope())
                    {
                        var context = new MethodInvocationContext { Invocation = invocation };

                        workContext.Resolve<IMethodInvocationEventHandler>().MethodInvoked(context);

                        if (context.CancelHardwareInvocation) return false;

                        // Implement FPGA communication, data transformation here.

                        return true;
                    }
                };
        }


        private class MethodInvocationContext : IMethodInvocationContext
        {
            public bool CancelHardwareInvocation { get; set; }
            public IInvocation Invocation { get; set; }
        }
    }
}
