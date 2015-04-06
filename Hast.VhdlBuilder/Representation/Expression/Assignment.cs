﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Assignment : IVhdlElement
    {
        public IDataObject AssignTo { get; set; }
        public IVhdlElement Expression { get; set; }


        public string ToVhdl()
        {
            return
                AssignTo.Name.ToExtendedVhdlId() +
                (AssignTo.DataObjectKind == DataObjectKind.Variable ? " := " : " <= ") +
                Expression.ToVhdl();
        }
    }
}
