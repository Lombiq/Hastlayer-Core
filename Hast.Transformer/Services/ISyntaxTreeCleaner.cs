using Hast.Layer;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Removes nodes from the syntax tree that aren't needed.
    /// </summary>
    public interface ISyntaxTreeCleaner : IDependency
    {
        void CleanUnusedDeclarations(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
