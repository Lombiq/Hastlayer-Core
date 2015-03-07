using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer
{
    public interface ITransformingEngine
    {
        IHardwareDefinition Transform(string id, SyntaxTree syntaxTree);
    }
}
