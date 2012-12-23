using ICSharpCode.NRefactory.CSharp;

namespace HastTranspiler
{
    public interface ITranspilingEngine
    {
        IHardwareDefinition Transpile(string id, SyntaxTree syntaxTree);
    }
}
