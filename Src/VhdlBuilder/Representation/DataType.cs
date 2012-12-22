﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation
{
    /// <summary>
    /// VHDL object data type, e.g. std_logic or std_logic_vector.
    /// </summary>
    public class DataType : IVhdlElement
    {
        public string Name { get; set; }

        public virtual string ToVhdl()
        {
            return Name;
        }
    }
}
