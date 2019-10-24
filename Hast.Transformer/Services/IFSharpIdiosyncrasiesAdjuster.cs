using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Adjusts constructs only coming from F# and changes them into their equivalents usual in a C# AST.
    /// </summary>
    public interface IFSharpIdiosyncrasiesAdjuster : IDependency
    {
        void AdjustFSharpIdiosyncrasie(SyntaxTree syntaxTree);
    }
}
