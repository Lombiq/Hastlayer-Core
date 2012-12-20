using ICSharpCode.NRefactory.CSharp;

namespace HastTranspiler
{
    public interface ITranspilingEngine
    {
        string Transpile(SyntaxTree syntaxTree);
    }
}
