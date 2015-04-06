﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class String : DataType
    {
        public int Length { get; set; }


        public String()
        {
            TypeCategory = DataTypeCategory.Array;
        }


        public override string ToVhdl()
        {
            return "string(1 to " + Length + ")";
        }
    }
}
