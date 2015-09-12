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


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminate(Element.ToVhdl(vhdlGenerationOptions), vhdlGenerationOptions);
        }


        public static string Terminator(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return ";" + vhdlGenerationOptions.NewLineIfShouldFormat();
        }

        public static string Terminate(string vhdl, IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return vhdl.TrimEnd(Environment.NewLine.ToCharArray()).EndsWith(";") ?
                vhdl :
                vhdl + Terminator(vhdlGenerationOptions);
        }
    }
}
