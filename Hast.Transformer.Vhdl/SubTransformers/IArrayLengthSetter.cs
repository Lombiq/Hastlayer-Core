using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Service for statically setting the size of arrays in cases this is not easy to determine.
    /// </summary>
    public interface IArrayLengthSetter : IDependency
    {
        void SetArrayParameterSizes(SyntaxTree syntaxTree);
    }
}
