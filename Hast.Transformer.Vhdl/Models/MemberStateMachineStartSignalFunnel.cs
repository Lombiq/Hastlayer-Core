using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Models
{
    internal class MemberStateMachineStartSignalFunnel : IMemberStateMachineStartSignalFunnel
    {
        // The type in the outer ConcurrentDictionary should be a ConcurrentHashSet but such collection doesn't exist yet.
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _startSignalsForStateMachines;


        public MemberStateMachineStartSignalFunnel()
        {
            _startSignalsForStateMachines = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>();
        }


        public void AddDrivingStartSignalForStateMachine(string signalName, string stateMachineName)
        {
            _startSignalsForStateMachines.TryAdd(stateMachineName, new ConcurrentDictionary<string, byte>());

            _startSignalsForStateMachines[stateMachineName].TryAdd(signalName, 0);
        }

        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> GetDrivingStartSignalsForStateMachines()
        {
            return _startSignalsForStateMachines.Select(kvp => 
                new KeyValuePair<string, IEnumerable<string>>(kvp.Key, kvp.Value.Keys));
        }
    }
}
