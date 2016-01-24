using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// A VHDL comment that produces its value when VHDL code is generated.
    /// </summary>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class GeneratedComment : IVhdlElement
    {
        private readonly Func<IVhdlGenerationOptions, string> _generator;


        public GeneratedComment(Func<IVhdlGenerationOptions, string> generator)
        {
            _generator = generator;
        }
        

        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return new Comment(_generator(vhdlGenerationOptions)).ToVhdl(vhdlGenerationOptions);
        }
    }
}
