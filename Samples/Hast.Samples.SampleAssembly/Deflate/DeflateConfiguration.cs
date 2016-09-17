using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly.Deflate
{
    internal struct DeflateConfiguration
    {
        public int good_length { get; set; } // reduce lazy search above this match length
        public int max_lazy { get; set; } // do not perform lazy search above this match length
        public int nice_length { get; set; } // quit search above this match length
        public int max_chain { get; set; }
    }
}
