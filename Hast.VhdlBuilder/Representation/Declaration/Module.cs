﻿using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Module : IVhdlElement
    {
        public List<Library> Libraries { get; set; }
        public Entity Entity { get; set; }
        public Architecture Architecture { get; set; }


        public Module()
        {
            Libraries = new List<Library>();
        }


        public string ToVhdl()
        {
            return Libraries.ToVhdl() + Entity.ToVhdl() + Architecture.ToVhdl();
        }
    }
}
