﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    public class Entity : IVhdlElement
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

    public class Port : IDataObject
    {
        public string Name { get; set; }
        public PortMode Mode { get; set; }
        public DataType DataType { get; set; }

        public string ToVhdl()
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
