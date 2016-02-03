using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Models
{
    internal class MemberStateMachineStartedSignalFunnel : IMemberStateMachineStartedSignalFunnel
    {
        // The type in the outer ConcurrentDictionary should be a ConcurrentHashSet but such collection doesn't exist yet.
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _startedSignalsForStateMachines;


        public MemberStateMachineStartedSignalFunnel()
        {
            _startedSignalsForStateMachines = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>();
        }


        public void AddDrivingStartedSignalForStateMachine(string signalName, string stateMachineName)
        {
            _startedSignalsForStateMachines.TryAdd(stateMachineName, new ConcurrentDictionary<string, byte>());

            _startedSignalsForStateMachines[stateMachineName].TryAdd(signalName, 0);
        }

        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> GetDrivingStartedSignalsForStateMachines()
        {
            return _startedSignalsForStateMachines.Select(kvp => 
                new KeyValuePair<string, IEnumerable<string>>(kvp.Key, kvp.Value.Keys));
        }
    }
}
