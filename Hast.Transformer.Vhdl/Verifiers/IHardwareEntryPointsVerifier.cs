using Hast.Common.Interfaces;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.Verifiers
{
    /// <summary>
    /// Checks if hardware entry point types are suitable for transforming.
    /// </summary>
    public interface IHardwareEntryPointsVerifier : IDependency
    {
        /// <summary>
        /// Checks if hardware entry point types are suitable for transforming.
        /// </summary>
        void VerifyHardwareEntryPoints(SyntaxTree syntaxTree, ITypeDeclarationLookupTable typeDeclarationLookupTable);
    }
}
