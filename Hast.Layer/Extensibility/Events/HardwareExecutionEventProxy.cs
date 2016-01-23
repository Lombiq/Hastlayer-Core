using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Models;
using Hast.Communication.Extensibility;
using Hast.Communication.Extensibility.Events;
using Orchard;

namespace Hast.Layer.Extensibility.Events
{
    public interface IHardwareExecutionEventProxy : ISingletonDependency
    {
        void RegisterExecutedOnHardwareEventHandler(Action<ExecutedOnHardwareEventArgs> eventHandler);
    }


    public class HardwareExecutionEventProxy : IHardwareExecutionEventProxy, IMemberInvocationEventHandler
    {
        private Action<ExecutedOnHardwareEventArgs> _eventHandler;


        public void RegisterExecutedOnHardwareEventHandler(Action<ExecutedOnHardwareEventArgs> eventHandler)
        {
            // No need for locking since this will only be run once in a shell.
            _eventHandler = eventHandler;
        }

        public void MemberInvoking(IMemberInvocationContext invocationContext) { }

        public void MemberExecutedOnHardware(IMemberHardwareExecutionContext hardwareExecutionContext)
        {
            _eventHandler(new ExecutedOnHardwareEventArgs(
                hardwareExecutionContext.HardwareRepresentation,
                hardwareExecutionContext.MemberFullName,
                hardwareExecutionContext.ExecutionInformation));
        }
    }
}
