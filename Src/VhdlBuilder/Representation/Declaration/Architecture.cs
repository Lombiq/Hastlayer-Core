﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder;

namespace VhdlBuilder.Representation.Declaration
{
    // Although by implementing INamedElement and IStructuredElement Architecture is in the end implementing ISubProgram. However the
    // architecture is not a subprogram, so implementing ISubProgram directly would be semantically incorrect.
    public class Architecture : INamedElement, IStructuredElement
    {
        public string Name { get; set; }
        public Entity Entity { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Architecture()
        {
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return
                "architecture " +
                Name.ToVhdlId() +
                " of " +
                Entity.Name.ToVhdlId() +
                " is " +
                Declarations.ToVhdl() +
                " begin " +
                Body.ToVhdl() +
                " end " +
                Name.ToVhdlId() +
                ";";
        }
    }
}
