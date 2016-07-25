﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IArrayTypesCreator : IDependency
    {
        IEnumerable<IVhdlElement> CreateArrayTypes(SyntaxTree syntaxTree);
    }
}
