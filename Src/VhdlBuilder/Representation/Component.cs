﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    public class Component : IVhdlElement
    {
        public string Name { get; set; }
        public List<Port> Ports { get; set; }


        public Component()
        {
            Ports = new List<Port>();
        }


        public string ToVhdl()
        {
            return
                "component " +
                Name.ToVhdlId() +
                " port(" +
                string.Join(", ", Ports.Select(parameter => parameter.ToVhdl())) +
                ");" +
                "end component;";
        }
    }
}
