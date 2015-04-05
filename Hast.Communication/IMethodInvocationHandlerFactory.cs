using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Hast.Common.Models;
using Orchard;

namespace Hast.Communication
{
    /// <summary>
    /// Delegate for handling method invocations of objects whose logic is implemented as hardware.
    /// </summary>
    /// <param name="invocation">The context of the method invocation.</param>
    /// <returns>
    /// <c>True</c> if the method invocation was successfully transferred to the hardware implementation, <c>false</c> otherwise.
    /// </returns>
    public delegate bool MethodInvocationHandler(IInvocation invocation);


    /// <summary>
    /// Creates delegates that will handle method invocations issued to methods of objects whose logic is implemented as hardware.
    /// </summary>
    public interface IMethodInvocationHandlerFactory : ISingletonDependency
    {
        MethodInvocationHandler CreateMethodInvocationHandler(IHardwareRepresentation hardwareRepresentation, object target);
    }
}
