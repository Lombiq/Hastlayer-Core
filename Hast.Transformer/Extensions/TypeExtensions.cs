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
    }
}
