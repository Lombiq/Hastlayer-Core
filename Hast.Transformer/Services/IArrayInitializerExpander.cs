using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts inline array initalizers into one-by-one array element assignments so these can be transformed in a
    /// simpler way.
    /// </summary>
    /// <example>
    /// var x = new[] { 1, 2, 3 };
    /// 
    /// will be converted to:
    /// 
    /// var x = new int[3];
    /// x[0] = 1;
    /// x[1] = 2;
    /// x[2] = 3;
    /// </example>
    public interface IArrayInitializerExpander : IDependency
    {
        void ExpandArrayInitializers(SyntaxTree syntaxTree);
    }
}
