using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    // Although by implementing INamedElement and IStructuredElement Architecture is in the end implementing ISubProgram. 
    // However the architecture is not a subprogram, so implementing ISubProgram directly would be semantically incorrect.
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Architecture : INamedElement, IStructuredElement
    {
        public string Name { get; set; }
        public Entity Entity { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Architecture()
        {
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            var name = vhdlGenerationOptions.ShortenName(Name);
            return Terminated.Terminate(
                "architecture " + name + " of " + vhdlGenerationOptions.ShortenName(Entity.Name) + " is " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Declarations.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "begin " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Body.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "end " + name, vhdlGenerationOptions);
        }
    }
}
