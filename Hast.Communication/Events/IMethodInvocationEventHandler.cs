using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Orchard.Events;

namespace Hast.Communication.Events
{
    /// <summary>
    /// The context of the invocation of a hardware-implemented method.
    /// </summary>
    public interface IMethodInvocationContext
    {
        /// <summary>
        /// If set to <c>true</c>, invoking the method on hardware will be cancelled and method invocation resumes in software.
        /// </summary>
        bool CancelHardwareInvocation { get; set; }

        /// <summary>
        /// Context of the method invocation.
        /// </summary>
        IInvocation Invocation { get; }
    }


    /// <summary>
    /// Event handler to hook into the of invoking hardware-implemented methods.
    /// </summary>
    public interface IMethodInvocationEventHandler : IEventHandler
    {
        /// <summary>
        /// Fired when a hardware-implemented method is being invoked.
        /// </summary>
        /// <param name="invocationContext">The context of the method invocation.</param>
        void MethodInvoked(IMethodInvocationContext invocationContext);
    }
}
