using System.Collections.Generic;
using System.Linq;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard;

namespace Hast.Transformer
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
                .GetTypes(true)
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
                TypeDeclaration declaration;
                _typeDeclarations.TryGetValue(fullName, out declaration);
                return declaration;
            }
        }
    }
}
