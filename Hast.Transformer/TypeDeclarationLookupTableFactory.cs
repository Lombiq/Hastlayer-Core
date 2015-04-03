using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
           var z = syntaxTree
                .GetTypes(true)
                .GroupBy(d => d.Annotation<TypeDefinition>().FullName);

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
        
        
            public TypeDeclaration Lookup(AstType type)
            {
                var typeReference = type.Annotation<TypeReference>();
                if (typeReference == null) return null;
                TypeDeclaration declaration;
                _typeDeclarations.TryGetValue(typeReference.FullName, out declaration);
                return declaration;
            }
        }
    }
}
