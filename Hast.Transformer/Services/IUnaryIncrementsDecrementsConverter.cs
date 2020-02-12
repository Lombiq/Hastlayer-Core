using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts unary pre/post-increment/decrement expressions into simple binary operator expression assignments.
    /// This is needed because such expressions will remain if not in their own statements (those will be simplified by
    /// ILSpy).
    /// </summary>
    /// <example>
    /// array[input++] = 3;
    /// 
    /// ...will be converted into:
    /// array[input++] = 3;
    /// </example>
    public interface IUnaryIncrementsDecrementsConverter : IDependency
    {
        void ConvertUnaryIncrementsDecrements(SyntaxTree syntaxTree);
    }
}
