using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Models
{
    /// <summary>
    /// When a state machine wants to start another state machine it should add its own start signal to a common funnel 
    /// that will then produce the actual start signal.
    /// </summary>
    /// <remarks>
    /// This is needed because the same signal can't be assigned to (driven) from multiple processes, so a single start
    /// signal that is assigned to from multiple state machines wouldn't work.
    /// </remarks>
    public interface IMemberStateMachineStartSignalFunnel
    {
        void AddDrivingStartSignalForStateMachine(string signalName, string stateMachineName);
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> GetDrivingStartSignalsForStateMachines();
    }
}
