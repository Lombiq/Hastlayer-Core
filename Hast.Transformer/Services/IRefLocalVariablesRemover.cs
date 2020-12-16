using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Removes ref locals and substitutes them with the original reference. This makes the AST simpler.
    /// </summary>
    /// <example>
    /// The variable "reference" can be substituted by an indexer expression to the array element it refers to.
    /// uint[] array;
    /// array = new uint[1];
    /// array [0] = num;
    /// ref uint reference = ref array[0];
    /// reference = reference >> 1;
    /// </example>
    public interface IRefLocalVariablesRemover : IDependency
    {
        void RemoveRefLocalVariables(SyntaxTree syntaxTree);
    }
}
