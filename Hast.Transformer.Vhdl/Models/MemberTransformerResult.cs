﻿using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.Models
{
    internal class MemberTransformerResult : IMemberTransformerResult
    {
        public EntityDeclaration Member { get; set; }
        public bool IsHardwareEntryPointMember { get; set; }
        public IEnumerable<IArchitectureComponentResult> ArchitectureComponentResults { get; set; }
    }
}
