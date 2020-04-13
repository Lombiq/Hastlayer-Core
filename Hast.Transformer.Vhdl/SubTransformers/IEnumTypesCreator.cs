﻿using System.Collections.Generic;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IEnumTypesCreator : IDependency
    {
        IEnumerable<IVhdlElement> CreateEnumTypes(SyntaxTree syntaxTree);
    }
}
