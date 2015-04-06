﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;
using System.Diagnostics;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Invokation : IVhdlElement
    {
        public IVhdlElement Target { get; set; }
        public List<IVhdlElement> Parameters { get; set; }


        public Invokation()
        {
            Parameters = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return 
                Target.ToVhdl() +
                (Parameters != null && Parameters.Any() ? "(" + string.Join(", ", Parameters.Select(parameter => parameter.ToVhdl())) + ")" : string.Empty);
        }
    }


    [DebuggerDisplay("{ToVhdl()}")]
    public class NamedInvokationParameter : IVhdlElement
    {
        public INamedElement FormalParameter { get; set; }
        public INamedElement ActualParameter { get; set; }


        public string ToVhdl()
        {
            return FormalParameter.Name.ToExtendedVhdlId() + " => " + ActualParameter.Name.ToExtendedVhdlId();
        }
    }
}
