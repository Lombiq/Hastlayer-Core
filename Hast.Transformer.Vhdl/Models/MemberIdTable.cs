using System;
using System.Collections.Generic;

namespace Hast.Transformer.Vhdl.Models
{
    /// <summary>
    /// Maps class members to generated IDs that the hardware-implemented logic uses. A member access in .NET is thus
    /// transferred as a call to a member ID and this member will determine which part of the logic will execute.
    /// </summary>
    public class MemberIdTable
    {
        private static MemberIdTable _emptyInstance;
        private readonly Dictionary<string, int> _mappings = new Dictionary<string, int>();

        public IReadOnlyDictionary<string, int> Mappings => _mappings;

        public static MemberIdTable Empty
        {
            get
            {
                if (_emptyInstance == null) _emptyInstance = new MemberIdTable();
                return _emptyInstance;
            }
        }

        public int MaxId { get; private set; }

        public void SetMapping(string memberFullName, int id)
        {
            if (id > MaxId) MaxId = id;
            if (!_mappings.ContainsKey(memberFullName))
            {
                _mappings[memberFullName] = id;
            }
            else _mappings[memberFullName] = id;
        }

        public int LookupMemberId(string memberFullName)
        {
            int id;
            if (_mappings.TryGetValue(memberFullName, out id)) return id;
            throw new InvalidOperationException("No member ID mapping found for the given member name: " + memberFullName);
        }
    }
}
