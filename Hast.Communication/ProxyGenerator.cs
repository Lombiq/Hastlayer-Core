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
        private readonly IMemberInvocationHandlerFactory _memberInvocationHandlerFactory;
        private readonly Castle.DynamicProxy.ProxyGenerator _proxyGenerator;


        public ProxyGenerator(IMemberInvocationHandlerFactory memberInvocationHandlerFactory)
        {
            _memberInvocationHandlerFactory = memberInvocationHandlerFactory;
            _proxyGenerator = new Castle.DynamicProxy.ProxyGenerator();
        }


        public T CreateCommunicationProxy<T>(IMaterializedHardware materializedHardware, T target) where T : class
        {
            var memberInvokationHandler = _memberInvocationHandlerFactory.CreateMemberInvocationHandler(materializedHardware, target);
            if (typeof(T).IsInterface)
            {
                return _proxyGenerator.CreateInterfaceProxyWithTarget<T>(target, new MemberInvocationInterceptor(memberInvokationHandler));
            }

            return _proxyGenerator.CreateClassProxyWithTarget<T>(target, new MemberInvocationInterceptor(memberInvokationHandler));
        }


        [Serializable]
        public class MemberInvocationInterceptor : Castle.DynamicProxy.IInterceptor
        {
            private readonly MemberInvocationHandler _memberInvocationHandler;


            public MemberInvocationInterceptor(MemberInvocationHandler memberInvocationHandler)
            {
                _memberInvocationHandler = memberInvocationHandler;
            }
        
        
            public void Intercept(Castle.DynamicProxy.IInvocation invocation)
            {
                if (!_memberInvocationHandler(invocation))
                {
                    invocation.Proceed();
                }
            }
        }
    }
}
