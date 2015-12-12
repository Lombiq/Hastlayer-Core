using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Models;

namespace Hast.Communication
{
    public class ProxyGenerator : IProxyGenerator
    {
        private readonly IMethodInvocationHandlerFactory _methodInvocationHandlerFactory;
        private readonly Castle.DynamicProxy.ProxyGenerator _proxyGenerator;


        public ProxyGenerator(IMethodInvocationHandlerFactory methodInvocationHandlerFactory)
        {
            _methodInvocationHandlerFactory = methodInvocationHandlerFactory;
            _proxyGenerator = new Castle.DynamicProxy.ProxyGenerator();
        }


        public T CreateCommunicationProxy<T>(IHardwareRepresentation hardwareRepresentation, T target) where T : class
        {
            var methodInvokationHandler = _methodInvocationHandlerFactory.CreateMethodInvocationHandler(hardwareRepresentation, target);
            if (typeof(T).IsInterface)
            {
                return _proxyGenerator.CreateInterfaceProxyWithTarget<T>(target, new MethodInvocationInterceptor(methodInvokationHandler));
            }

            return _proxyGenerator.CreateClassProxyWithTarget<T>(target, new MethodInvocationInterceptor(methodInvokationHandler));
        }


        [Serializable]
        public class MethodInvocationInterceptor : Castle.DynamicProxy.IInterceptor
        {
            private readonly MethodInvocationHandler _methodInvocationHandler;


            public MethodInvocationInterceptor(MethodInvocationHandler methodInvocationHandler)
            {
                _methodInvocationHandler = methodInvocationHandler;
            }
        
        
            public void Intercept(Castle.DynamicProxy.IInvocation invocation)
            {
                if (!_methodInvocationHandler(invocation))
                {
                    invocation.Proceed();
                }
            }
        }
    }
}
