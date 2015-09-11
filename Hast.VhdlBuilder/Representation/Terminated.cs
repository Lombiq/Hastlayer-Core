using System;

namespace Hast.VhdlBuilder.Representation
{
    public class Terminated : IVhdlElement
    {
        public IVhdlElement Element { get; set; }


        public Terminated()
        {
        }

        public Terminated(IVhdlElement element)
        {
            Element = element;
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return Terminate(Element.ToVhdl(vhdlGenerationContext), vhdlGenerationContext);
        }


        public static string Terminate(string vhdl, IVhdlGenerationContext vhdlGenerationContext)
        {
            return vhdl.TrimEnd(Environment.NewLine.ToCharArray()).EndsWith(";") ?
                vhdl :
                vhdl + ";" + vhdlGenerationContext.NewLineIfShouldFormat();
        }
    }
}
