using Orchard.Events;

namespace Hast.Communication.Extensibility.Events
{
    /// <summary>
    /// Event handler to hook into the of invoking hardware-implemented methods.
    /// </summary> 
    public interface IMethodInvocationEventHandler : IEventHandler
    {
        /// <summary>
        /// Fired when a hardware-implemented method is being invoked.
        /// </summary>
        /// <param name="invocationContext">The context of the method invocation.</param>
        void MethodInvoking(IMethodInvocationContext invocationContext);

        /// <summary>
        /// Fired when a hardware-implemented method finished being invoked as hardware-implemented logic.
        /// </summary>
        /// <param name="invocationContext">The context of the method invocation.</param>
        void MethodInvokedOnHardware(IMethodInvocationContext invocationContext);
    }
}
