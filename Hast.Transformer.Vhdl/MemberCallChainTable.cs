using System.Collections.Generic;

namespace Hast.Transformer.Vhdl
{
    public class MemberCallChain
    {
        public string ProcedureName { get; set; }
        public List<MemberCallChain> Targets { get; set; }

        public MemberCallChain()
        {
            Targets = new List<MemberCallChain>();
        }
    }


    /// <summary>
    /// Contains information on which member calls which other members(s). This is needed to then re-order VHDL procedures
    /// if necessary.
    /// </summary>
    public class MemberCallChainTable
    {
        private readonly Dictionary<string, MemberCallChain> _chains = new Dictionary<string, MemberCallChain>();

        public IDictionary<string, MemberCallChain> Chains { get { return _chains; } }


        public void AddTarget(string procedureName, string targetProcedureName)
        {
            if (!_chains.ContainsKey(procedureName)) _chains[procedureName] = new MemberCallChain { ProcedureName = procedureName };
            if (!_chains.ContainsKey(targetProcedureName)) _chains[targetProcedureName] = new MemberCallChain { ProcedureName = targetProcedureName };
            
            var member = _chains[procedureName];
            var target = _chains[targetProcedureName];
            if (!member.Targets.Contains(target)) member.Targets.Add(target);
        }
    }
}
