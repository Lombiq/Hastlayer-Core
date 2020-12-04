using Hast.Transformer.Abstractions.SimpleMemory;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem.Implementation;
using System;
using System.Linq;

namespace ICSharpCode.Decompiler.TypeSystem
{
    public static class TypeExtensions
    {
        public static KnownTypeCode? GetKnownTypeCode(this IType type) => (type as ITypeDefinition)?.KnownTypeCode;

        public static bool IsPrimitive(this IType type)
        {
            var typeCode = type.GetKnownTypeCode();
            return
                typeCode == KnownTypeCode.Boolean ||
                typeCode == KnownTypeCode.Byte ||
                typeCode == KnownTypeCode.Char ||
                typeCode == KnownTypeCode.Decimal ||
                typeCode == KnownTypeCode.Double ||
                typeCode == KnownTypeCode.Int16 ||
                typeCode == KnownTypeCode.Int32 ||
                typeCode == KnownTypeCode.Int64 ||
                typeCode == KnownTypeCode.Object ||
                typeCode == KnownTypeCode.SByte ||
                typeCode == KnownTypeCode.Single ||
                typeCode == KnownTypeCode.String ||
                typeCode == KnownTypeCode.UInt16 ||
                typeCode == KnownTypeCode.UInt32 ||
                typeCode == KnownTypeCode.UInt64 ||
                typeCode == KnownTypeCode.Void;
        }

        public static bool IsArray(this IType type) => type.Kind == TypeKind.Array;

        public static bool IsAttribute(this IType type) =>
            // ILSpy has such an IsAttributeType() method but it's private.
            type.GetNonInterfaceBaseTypes().Any(t => t.IsKnownType(KnownTypeCode.Attribute));

        public static bool IsClass(this IType type) => type.Kind == TypeKind.Class;

        public static bool IsEnum(this IType type) => type.Kind == TypeKind.Enum;

        public static bool IsFunc(this IType type) => type.FullName.StartsWith("System.Func", StringComparison.Ordinal);

        public static bool IsSimpleMemory(this IType type) => type.GetFullName() == typeof(SimpleMemory).FullName;

        public static bool IsStruct(this IType type) => type.Kind == TypeKind.Struct;

        public static IType GetElementType(this IType type) =>
            type is TypeWithElementType typeWithElementType ? typeWithElementType.ElementType : null;

        public static ResolveResult ToResolveResult(this IType type) => new ResolveResult(type);

        // For nested types the conventional separator is a slash.
        public static string GetFullName(this IType type) => type.ReflectionName;
    }
}
