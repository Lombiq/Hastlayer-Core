using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    // Although by implementing INamedElement and IStructuredElement Architecture is in the end implementing ISubProgram. However the
    // architecture is not a subprogram, so implementing ISubProgram directly would be semantically incorrect.
    [DebuggerDisplay("{ToVhdl()}")]
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


        public string ToVhdl()
        {
            return
                "architecture " +
                Name.ToExtendedVhdlId() +
                " of " +
                Entity.Name + // Entity names can't be extended identifiers.
                " is " +
                Declarations.ToVhdl() +
                " begin " +
                Body.ToVhdl() +
                " end " +
                Name.ToExtendedVhdlId() +
                ";";
        }
    }
}
