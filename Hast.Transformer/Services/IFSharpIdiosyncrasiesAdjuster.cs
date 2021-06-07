using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Adjusts constructs only coming from F# and changes them into their equivalents usual in a C# AST.
    /// </summary>
    public interface IFSharpIdiosyncrasiesAdjuster : IDependency
    {
        void AdjustFSharpIdiosyncrasies(SyntaxTree syntaxTree);
    }
}
