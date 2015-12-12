using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Hast.Transformer.Vhdl
{
    public class MemberCallChain
    {
        public string MemberName { get; set; }
        public List<MemberCallChain> Targets { get; set; }

        public MemberCallChain()
        {
            Targets = new List<MemberCallChain>();
        }
    }


    /// <summary>
    /// Contains information on which member calls which other members(s).
    /// </summary>
    public class MemberCallChainTable
    {
        private readonly ConcurrentDictionary<string, MemberCallChain> _chains = new ConcurrentDictionary<string, MemberCallChain>();

        public IDictionary<string, MemberCallChain> Chains { get { return _chains; } }


        public void AddTarget(string memberName, string targetMemberName)
        {
            if (!_chains.ContainsKey(memberName)) _chains[memberName] = new MemberCallChain { MemberName = memberName };
            if (!_chains.ContainsKey(targetMemberName)) _chains[targetMemberName] = new MemberCallChain { MemberName = targetMemberName };
            
            var member = _chains[memberName];
            var target = _chains[targetMemberName];
            if (!member.Targets.Contains(target)) member.Targets.Add(target);
        }
    }
}
