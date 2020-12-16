using Hast.VhdlBuilder.Extensions;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    // Although by implementing INamedElement and IStructuredElement Architecture is in the end implementing
    // ISubProgram. However, the architecture is not a subprogram, so implementing ISubProgram directly would be
    // semantically incorrect.
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Architecture : INamedElement, IStructuredElement, IReferenceableDeclaration<Architecture.ArchitectureReference>
    {
        public string Name { get; set; }
        public Entity Entity { get; set; }
        public List<IVhdlElement> Declarations { get; set; } = new List<IVhdlElement>();
        public List<IVhdlElement> Body { get; set; } = new List<IVhdlElement>();


        public ArchitectureReference ToReference() => new ArchitectureReference { Name = Name };

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


        [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
        public class ArchitectureReference : INamedElement
        {
            public string Name { get; set; }

            public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) => vhdlGenerationOptions.ShortenName(Name);
        }
    }
}
