using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl
{
    public class CallChain
    {
        public string ProcedureName { get; set; }
        public List<CallChain> Targets { get; set; }

        public CallChain()
        {
            Targets = new List<CallChain>();
        }
    }

    public class CallChainTable
    {
        private readonly Dictionary<string, CallChain> _chains = new Dictionary<string, CallChain>();

        public IEnumerable<CallChain> Values { get { return _chains.Values; } }


        public void AddTarget(string procedureName, string targetProcedureName)
        {
            if (!_chains.ContainsKey(procedureName)) _chains[procedureName] = new CallChain { ProcedureName = procedureName };
            if (!_chains.ContainsKey(targetProcedureName)) _chains[targetProcedureName] = new CallChain { ProcedureName = targetProcedureName };
            
            var method = _chains[procedureName];
            var target = _chains[targetProcedureName];
            if (!method.Targets.Contains(target)) method.Targets.Add(target);
        }
    }
}
