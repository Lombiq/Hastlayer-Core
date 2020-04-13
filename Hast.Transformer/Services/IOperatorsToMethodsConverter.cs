using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

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
