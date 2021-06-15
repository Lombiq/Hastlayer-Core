using Hast.Common.Interfaces;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    public interface ITypeDeclarationLookupTableFactory : IDependency
    {
        ITypeDeclarationLookupTable Create(SyntaxTree syntaxTree);
    }
}
