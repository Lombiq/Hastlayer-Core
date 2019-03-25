using ICSharpCode.NRefactory.CSharp;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Fixes errors happening during decompilation introduced by ILSpy bugs. Primarily fixing 
    /// https://github.com/icsharpcode/ILSpy/issues/807.
    /// </summary>
    public interface IDecompilationErrorsFixer : IDependency
    {
        void FixDecompilationErrors(SyntaxTree syntaxTree);
    }
}
