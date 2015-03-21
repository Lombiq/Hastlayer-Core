using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public class Entity : INamedElement
    {
        public string Name { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<Port> Ports { get; set; }


        public Entity()
        {
            Ports = new List<Port>();
            Declarations = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return
                "entity " +
                Name.ToVhdlId() +
                " is port(" +
                string.Join("; ", Ports.Select(parameter => parameter.ToVhdl())) +
                ");" +
                Declarations.ToVhdl() +
                "end " +
                Name.ToVhdlId() +
                ";";
        }
    }


    public enum PortMode
    {
        In,
        Out,
        Buffer,
        InOut
    }


    public class Port : DataObjectBase
    {
        public PortMode Mode { get; set; }

        public Port()
        {
            this.ObjectType = ObjectType.Signal;
        }

        public override string ToVhdl()
        {
            return
                Name.ToVhdlId() +
                ": " +
                Mode +
                " " +
                DataType.ToVhdl();
        }
    }
}
