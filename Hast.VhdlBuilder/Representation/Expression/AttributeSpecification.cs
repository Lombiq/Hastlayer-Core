using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class AttributeSpecification : IVhdlElement
    {
        public Declaration.Attribute Attribute { get; set; }
        public string ItemName { get; set; }
        public string ItemClass { get; set; }
        public IVhdlElement Expression { get; set; }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            Terminated.Terminate(
                "attribute " + Attribute.ToReference().ToVhdl(vhdlGenerationOptions) + " of " + ItemName + ": " +
                ItemClass + " is " + Expression.ToVhdl(vhdlGenerationOptions), vhdlGenerationOptions);
    }
}
