using System;
using Hast.Transformer.Vhdl.Helpers;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;
using Orchard;
using System.Linq;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class TypeConverter : ITypeConverter
    {
        public DataType ConvertTypeReference(TypeReference typeReference)
        {
            if (typeReference.IsArray)
            {
                return CreateArrayType(ConvertTypeReference(typeReference.GetElementType()));
            }

            switch (typeReference.FullName)
            {
                case "System.Boolean":
                    return ConvertPrimitive(KnownTypeCode.Boolean);
                case "System.Char":
                    return ConvertPrimitive(KnownTypeCode.Char);
                case "System.Decimal":
                    return ConvertPrimitive(KnownTypeCode.Decimal);
                case "System.Double":
                    return ConvertPrimitive(KnownTypeCode.Double);
                case "System.Int16":
                    return ConvertPrimitive(KnownTypeCode.Int16);
                case "System.Int32":
                    return ConvertPrimitive(KnownTypeCode.Int32);
                case "System.Int64":
                    return ConvertPrimitive(KnownTypeCode.Int64);
                case "System.Object":
                    return ConvertPrimitive(KnownTypeCode.Object);
                case "System.String":
                    return ConvertPrimitive(KnownTypeCode.String);
                case "System.UInt16":
                    return ConvertPrimitive(KnownTypeCode.UInt16);
                case "System.UInt32":
                    return ConvertPrimitive(KnownTypeCode.UInt32);
                case "System.UInt64":
                    return ConvertPrimitive(KnownTypeCode.UInt64);
            }

            throw new NotSupportedException("The type " + typeReference.FullName + " is not supported for transforming.");
        }

        public DataType ConvertAstType(AstType type)
        {
            if (type is PrimitiveType) return ConvertPrimitive((type as PrimitiveType).KnownTypeCode);
            else if (type is ComposedType) return ConvertComposed((ComposedType)type);
            else if (type is SimpleType) return ConvertSimple((SimpleType)type);

            throw new NotSupportedException("The type " + type.ToString() + " is not supported for transforming.");
        }

        public DataType ConvertAndDeclareAstType(AstType type, IDeclarableElement declarable)
        {
            var vhdlType = ConvertAstType(type);

            if (vhdlType.TypeCategory == DataTypeCategory.Array || vhdlType.TypeCategory == DataTypeCategory.Composite)
            {
                declarable.Declarations.Add(vhdlType);
            }

            return vhdlType;
        }


        private DataType ConvertPrimitive(KnownTypeCode typeCode)
        {
            switch (typeCode)
            {
                case KnownTypeCode.Array:
                    break;
                case KnownTypeCode.Attribute:
                    break;
                case KnownTypeCode.Boolean:
                    return KnownDataTypes.Boolean;
                case KnownTypeCode.Byte:
                    break;
                case KnownTypeCode.Char:
                    return KnownDataTypes.Character;
                case KnownTypeCode.DBNull:
                    break;
                case KnownTypeCode.DateTime:
                    break;
                case KnownTypeCode.Decimal:
                    break;
                case KnownTypeCode.Delegate:
                    break;
                case KnownTypeCode.Double:
                    return KnownDataTypes.Real;
                case KnownTypeCode.Enum:
                    break;
                case KnownTypeCode.Exception:
                    break;
                case KnownTypeCode.ICollection:
                    break;
                case KnownTypeCode.ICollectionOfT:
                    break;
                case KnownTypeCode.IDisposable:
                    break;
                case KnownTypeCode.IEnumerable:
                    break;
                case KnownTypeCode.IEnumerableOfT:
                    break;
                case KnownTypeCode.IEnumerator:
                    break;
                case KnownTypeCode.IEnumeratorOfT:
                    break;
                case KnownTypeCode.IList:
                    break;
                case KnownTypeCode.IListOfT:
                    break;
                case KnownTypeCode.IReadOnlyListOfT:
                    break;
                case KnownTypeCode.Int16:
                    return KnownDataTypes.Int16;
                case KnownTypeCode.Int32:
                    return KnownDataTypes.Int32;
                case KnownTypeCode.Int64:
                    return KnownDataTypes.Int64;
                case KnownTypeCode.IntPtr:
                    break;
                case KnownTypeCode.MulticastDelegate:
                    break;
                case KnownTypeCode.None:
                    break;
                case KnownTypeCode.NullableOfT:
                    break;
                case KnownTypeCode.Object:
                    return KnownDataTypes.StdLogicVector32;
                case KnownTypeCode.SByte:
                    break;
                case KnownTypeCode.Single:
                    break;
                case KnownTypeCode.String:
                    return KnownDataTypes.String;
                case KnownTypeCode.Task:
                    break;
                case KnownTypeCode.TaskOfT:
                    break;
                case KnownTypeCode.Type:
                    break;
                case KnownTypeCode.UInt16:
                    return KnownDataTypes.UInt16;
                case KnownTypeCode.UInt32:
                    return KnownDataTypes.UInt32;
                case KnownTypeCode.UInt64:
                    return KnownDataTypes.UInt64;
                case KnownTypeCode.UIntPtr:
                    break;
                case KnownTypeCode.ValueType:
                    break;
                case KnownTypeCode.Void:
                    return KnownDataTypes.Void;
            }

            throw new NotSupportedException("The type " + typeCode.ToString() + " is not supported for transforming.");
        }

        private DataType ConvertComposed(ComposedType type)
        {
            if (type.ArraySpecifiers.Count != 0)
            {
                return CreateArrayType(ConvertAstType(type.BaseType));
            }

            throw new NotSupportedException("The type " + type.ToString() + " is not supported for transforming.");
        }

        private DataType ConvertSimple(SimpleType type)
        {
            if (type.Identifier == nameof(System.Threading.Tasks.Task))
            {
                // Changing e.g. Task<bool> to bool. Then it will be handled later what to do with the Task.
                if (type.TypeArguments.Count == 1)
                {
                    return ConvertAstType(type.TypeArguments.Single());
                }

                return SpecialTypes.Task;
            }

            throw new NotSupportedException("The type " + type.ToString() + " is not supported for transforming.");
        }

        private DataType CreateArrayType(DataType elementType)
        {
            return new VhdlBuilder.Representation.Declaration.ArrayType
            {
                ElementType = elementType,
                Name = ArrayTypeNameHelper.CreateArrayTypeName(elementType.Name)
            };
        }
    }
}
