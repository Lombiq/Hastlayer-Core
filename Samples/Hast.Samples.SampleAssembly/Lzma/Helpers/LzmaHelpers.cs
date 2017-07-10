using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly.Lzma.Helpers
{
    public static class LzmaHelpers
    {
        public static uint GetMinValue(uint first, uint second) =>
            first <= second ? first : second;
    }
}
