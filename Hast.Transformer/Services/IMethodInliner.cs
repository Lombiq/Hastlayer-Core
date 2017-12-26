using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Inlines suitable methods at the location of their invocation.
    /// </summary>
    public interface IMethodInliner : IDependency
    {
        void InlineMethods(SyntaxTree syntaxTree);
    }
}
