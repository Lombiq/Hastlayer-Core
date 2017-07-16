using System;
using System.Reflection;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;

namespace Hast.Transformer.Helpers
{
    public static class TypeHelper
    {
        public static TypeInformation CreateInt32TypeInformation() =>
            CreatePrimitiveTypeReference("Int32").ToTypeInformation();

        public static TypeReference CreatePrimitiveTypeReference(string typeName)
        {
            var int32Assembly = typeof(int).Assembly;
            var int32TypeReference = new TypeReference(
                "System",
                typeName,
                ModuleDefinition.CreateModule("System", new ModuleParameters()),
                new AssemblyNameReference(
                    int32Assembly.ShortName(),
                    new Version(int32Assembly.FullName.Split(',')[1].Substring(9))));
            int32TypeReference.IsValueType = true;
            return int32TypeReference;
        }
    }
}
