using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.TestInputs.Static
{
    /// <summary>
    /// Cases which ILSpy decompiles incorrectly, but Hastlayer works around the bugs.
    /// </summary>
    public class WronglyDecompiledCases
    {
        public void IncorrectlyDecompiledLiterals()
        {
            var a = (uint)ushort.MaxValue * ushort.MaxValue;
            var b = a + 4;

            var c = 0xFFFEB81BUL;
            var d = c + 4;
        }
    }
}
