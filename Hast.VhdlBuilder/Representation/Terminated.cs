
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


        public string ToVhdl()
        {
            var vhdl = Element.ToVhdl();
            return vhdl.EndsWith(";") ? vhdl : vhdl + ";";
        }
    }
}
