using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastTranspiler.Vhdl
{
    public class CallMapping
    {
        public string MethodName { get; set; }
        public int Id { get; set; }
    }

    public class CallIdTable
    {
        private static CallIdTable _emptyInstance;
        private readonly Dictionary<string, CallMapping> _mappings = new Dictionary<string, CallMapping>();

        public static CallIdTable Empty
        {
            get
            {
                if (_emptyInstance == null) _emptyInstance = new CallIdTable();
                return _emptyInstance;
            }
        }

        public int MaxId { get; private set; }

        public IEnumerable<CallMapping> Values { get { return _mappings.Values; } }


        public void SetMapping(string methodName, int id)
        {
            if (id > MaxId) MaxId = id;
            if (!_mappings.ContainsKey(methodName)) _mappings[methodName] = new CallMapping { MethodName = methodName, Id = id };
            else _mappings[methodName].Id = id;
        }
    }
}
