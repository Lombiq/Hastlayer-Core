﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
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
            return Element.ToVhdl() + ";";
        }
    }
}
