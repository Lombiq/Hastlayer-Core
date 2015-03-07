using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer
{
    public interface ITranspilingEngine
    {
        IHardwareDefinition Transpile(string id, SyntaxTree syntaxTree);
    }
}
