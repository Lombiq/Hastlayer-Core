using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.Verifiers
{
    /// <summary>
    /// Verifies compiler-generated classes for transformation, checking for potential issues preventing processing.
    /// </summary>
    public interface ICompilerGeneratedClassesVerifier : IDependency
    {
        /// <summary>
        /// Verifies compiler-generated classes for transformation, checking for potential issues preventing processing.
        /// </summary>
        void VerifyCompilerGeneratedClasses(SyntaxTree syntaxTree);
    }
}
