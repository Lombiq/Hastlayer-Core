using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl
{
    public class MethodCallChain
    {
        public string ProcedureName { get; set; }
        public List<MethodCallChain> Targets { get; set; }

        public MethodCallChain()
        {
            Targets = new List<MethodCallChain>();
        }
    }


    /// <summary>
    /// Contains information on which method calls which other method(s). This is needed to then re-order VHDL procedures if necessary.
    /// </summary>
    public class MethodCallChainTable
    {
        private readonly Dictionary<string, MethodCallChain> _chains = new Dictionary<string, MethodCallChain>();

        public IDictionary<string, MethodCallChain> Chains { get { return _chains; } }


        public void AddTarget(string procedureName, string targetProcedureName)
        {
            if (!_chains.ContainsKey(procedureName)) _chains[procedureName] = new MethodCallChain { ProcedureName = procedureName };
            if (!_chains.ContainsKey(targetProcedureName)) _chains[targetProcedureName] = new MethodCallChain { ProcedureName = targetProcedureName };
            
            var method = _chains[procedureName];
            var target = _chains[targetProcedureName];
            if (!method.Targets.Contains(target)) method.Targets.Add(target);
        }
    }
}
