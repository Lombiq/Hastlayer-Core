using ICSharpCode.NRefactory.CSharp;
using Orchard;

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
