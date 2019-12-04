using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts operator overloads into standard methods.
    /// </summary>
    public interface IOperatorsToMethodsConverter : IDependency
    {
        void ConvertOperatorsToMethods(SyntaxTree syntaxTree);
    }
}
