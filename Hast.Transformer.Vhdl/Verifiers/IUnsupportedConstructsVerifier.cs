using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.Verifiers
{
    /// <summary>
    /// Verifies whether unsupported languages constructs not checked for otherwise are present, and if yes, throws
    /// exceptions. Note: this doesn't check for everything unsupported.
    /// </summary>
    public interface IUnsupportedConstructsVerifier : IDependency
    {
        void ThrowIfUnsupportedConstructsFound(SyntaxTree syntaxTree);
    }
}
