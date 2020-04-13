using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.Verifiers
{
    /// <summary>
    /// Verifies compiler-generated classes for transformation, checking for potential issues preventing processing.
    /// </summary>
    public interface ICompilerGeneratedClassesVerifier : IDependency
    {
        void VerifyCompilerGeneratedClasses(SyntaxTree syntaxTree);
    }
}
