using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

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
