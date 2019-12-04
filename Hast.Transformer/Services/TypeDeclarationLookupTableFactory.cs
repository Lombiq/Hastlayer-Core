using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Transformer.Services
{
    public interface ITypeDeclarationLookupTableFactory : IDependency
    {
        ITypeDeclarationLookupTable Create(SyntaxTree syntaxTree);
    }


    public class TypeDeclarationLookupTableFactory : ITypeDeclarationLookupTableFactory
    {
        public ITypeDeclarationLookupTable Create(SyntaxTree syntaxTree)
        {
            var typeDeclarations = syntaxTree
                .GetAllTypeDeclarations()
                .ToDictionary(d => d.Annotation<TypeDefinition>().FullName);

            return new TypeDeclarationLookupTable(typeDeclarations);
        }


        private class TypeDeclarationLookupTable : ITypeDeclarationLookupTable
        {
            private readonly Dictionary<string, TypeDeclaration> _typeDeclarations;


            public TypeDeclarationLookupTable(Dictionary<string, TypeDeclaration> typeDeclarations)
            {
                _typeDeclarations = typeDeclarations;
            }


            public TypeDeclaration Lookup(string fullName)
            {
                _typeDeclarations.TryGetValue(fullName, out var declaration);
                return declaration;
            }
        }
    }
}
