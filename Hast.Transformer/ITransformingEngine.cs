using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer
{
    public interface ITransformingEngine
    {
        IHardwareDefinition Transform(string id, SyntaxTree syntaxTree);
    }
}
