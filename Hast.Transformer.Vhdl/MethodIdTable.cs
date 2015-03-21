using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl
{
    public class MethodMapping
    {
        public string MethodName { get; set; }
        public int Id { get; set; }
    }


    /// <summary>
    /// Maps methods to generated IDs that the hardware-implemented logic uses. A method call in .NET is thus transferred as a call to a
    /// method ID and this method will determine which part of the logic will execute.
    /// </summary>
    public class MethodIdTable
    {
        private static MethodIdTable _emptyInstance;
        private readonly Dictionary<string, MethodMapping> _mappings = new Dictionary<string, MethodMapping>();

        public static MethodIdTable Empty
        {
            get
            {
                if (_emptyInstance == null) _emptyInstance = new MethodIdTable();
                return _emptyInstance;
            }
        }

        public int MaxId { get; private set; }

        public IEnumerable<MethodMapping> Values { get { return _mappings.Values; } }


        public void SetMapping(string methodName, int id)
        {
            if (id > MaxId) MaxId = id;
            if (!_mappings.ContainsKey(methodName)) _mappings[methodName] = new MethodMapping { MethodName = methodName, Id = id };
            else _mappings[methodName].Id = id;
        }
    }
}
