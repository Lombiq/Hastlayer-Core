﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation.Declaration
{
    public interface IDeclarableElement : IVhdlElement
    {
        List<IVhdlElement> Declarations { get; set; }
    }
}
