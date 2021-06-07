using Hast.Common.Interfaces;
using Hast.Layer;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Inlines suitable methods at the location of their invocation.
    /// </summary>
    public interface IMethodInliner : IDependency
    {
        void InlineMethods(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
