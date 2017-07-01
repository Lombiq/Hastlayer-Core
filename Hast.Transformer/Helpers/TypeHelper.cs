using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Helpers
{
    internal static class TypeHelper
    {
        public static TypeInformation CreateInt32TypeInformation(AstNode node)
        {
            var int32Assembly = typeof(int).Assembly;
            var int32TypeReference = new TypeReference(
                "System",
                "Int32",
                node.FindFirstParentTypeDeclaration().Annotation<TypeDefinition>().Module,
                new AssemblyNameReference(
                    int32Assembly.ShortName(),
                    new Version(int32Assembly.FullName.Split(',')[1].Substring(9))));
            int32TypeReference.IsValueType = true;
            return new TypeInformation(int32TypeReference, int32TypeReference);
        }
    }
}
