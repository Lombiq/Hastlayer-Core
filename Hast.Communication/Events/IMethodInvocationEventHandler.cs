using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Hast.Common.Models;
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
        /// Gets the context of the method invocation.
        /// </summary>
        IInvocation Invocation { get; }

        /// <summary>
        /// Gets the full name of the invoked method, including the full namespace of the parent type(s) as well as their return type 
        /// and the types of their (type) arguments.
        /// </summary>
        string MethodFullName { get; }

        /// <summary>
        /// Gets the hardware representation behind the hardware-implemented members.
        /// </summary>
        IHardwareRepresentation HardwareRepresentation { get; }
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
