﻿using Hast.Transformer.Abstractions.SimpleMemory;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem.Implementation;

namespace ICSharpCode.Decompiler.TypeSystem
{
    public static class TypeExtensions
    {
        public static bool IsPrimitive(this IType type)
        {
            var typeCode = (type as ITypeDefinition)?.KnownTypeCode;
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

        public static bool IsEnum(this IType type) => type.Kind == TypeKind.Enum;

        public static bool IsSimpleMemory(this IType type) => type.GetFullName() == typeof(SimpleMemory).FullName;

        public static IType GetElementType(this IType type) =>
            type is TypeWithElementType typeWithElementType ? typeWithElementType.ElementType : null;

        public static ResolveResult ToResolveResult(this IType type) => new ResolveResult(type);

        public static string GetFullName(this IType type)
        {
            var declaringTypeNames = string.Empty;
            var currentType = type;
            while (currentType.DeclaringType != null)
            {
                // For nested types the conventional separator is a slash.
                declaringTypeNames = "/" + currentType.Name + declaringTypeNames;
                currentType = currentType.DeclaringType;
            }

            return currentType.FullName + declaringTypeNames;
        }
    }
}
