﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.Models
{
    public interface IArchitectureComponentResult
    {
        IArchitectureComponent ArchitectureComponent { get; }
        IVhdlElement Declarations { get; }
        IVhdlElement Body { get; }
    }
}