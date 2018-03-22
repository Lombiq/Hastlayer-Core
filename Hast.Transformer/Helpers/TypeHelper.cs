using System;
using System.Reflection;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;

namespace Hast.Transformer.Helpers
{
    public static class TypeHelper
    {
        public static TypeReference CreateInt32TypeReference() => CreatePrimitiveTypeReference("Int32");

        public static TypeInformation CreateInt32TypeInformation() => CreateInt32TypeReference().ToTypeInformation();

        public static TypeReference CreateUInt32TypeReference() => CreatePrimitiveTypeReference("UInt32");

        public static TypeInformation CreateUInt32TypeInformation() => CreateUInt32TypeReference().ToTypeInformation();

        public static TypeReference CreateInt64TypeReference() => CreatePrimitiveTypeReference("Int64");

        public static TypeInformation CreateInt64TypeInformation() => CreateInt64TypeReference().ToTypeInformation();

        public static TypeReference CreatePrimitiveTypeReference(string typeName)
        {
            var int32Assembly = typeof(int).Assembly;
            var int32TypeReference = new PrimitiveTypeReference(
                "System",
                typeName,
                ModuleDefinition.CreateModule("System", new ModuleParameters()),
                new AssemblyNameReference(
                    int32Assembly.ShortName(),
                    new Version(int32Assembly.FullName.Split(',')[1].Substring(9))));
            return int32TypeReference;
        }


        private class PrimitiveTypeReference : TypeReference
        {
            public PrimitiveTypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope)
                : base(@namespace, name, module, scope, true)
            {
            }


            public override bool IsPrimitive => true;
        }
    }
}
