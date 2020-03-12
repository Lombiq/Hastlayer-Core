﻿using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Vhdl.Verifiers
{
    /// <summary>
    /// Checks if hardware entry point types are suitable for transforming.
    /// </summary>
    public interface IHardwareEntryPointsVerifier : IDependency
    {
        void VerifyHardwareEntryPoints(SyntaxTree syntaxTree, ITypeDeclarationLookupTable typeDeclarationLookupTable);
    }
}
