using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastTranspiler.Vhdl
{
    public class CallIdTable : IEnumerable<KeyValuePair<string, int>>
    {
        private static CallIdTable _emptyInstance;
        private readonly Dictionary<string, int> _mappings = new Dictionary<string, int>();

        public static CallIdTable Empty
        {
            get
            {
                if (_emptyInstance == null) _emptyInstance = new CallIdTable();
                return _emptyInstance;
            }
        }

        public int this[string methodName]
        {
            get { return _mappings[methodName]; }

            set
            {
                if (value > MaxId) MaxId = value;
                _mappings[methodName] = value;
            }
        }

        public int MaxId { get; private set; }


        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return _mappings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
