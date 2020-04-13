using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

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
