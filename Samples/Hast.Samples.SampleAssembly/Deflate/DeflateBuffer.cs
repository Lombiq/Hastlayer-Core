using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly.Deflate
{
    internal class DeflateBuffer
    {
        //public int next { get; set; }
        public int len { get; set; }
        public byte[] ptr { get; set; } = new byte[DeflateCompressor.zip_OUTBUFSIZ];
        public int off { get; set; }
    }
}
