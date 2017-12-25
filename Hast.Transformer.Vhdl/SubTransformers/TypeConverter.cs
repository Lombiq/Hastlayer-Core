using System;
using System.Linq;
using Hast.Transformer.Helpers;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class TypeConverter : ITypeConverter
    {
        private readonly IRecordComposer _recordComposer;


        public TypeConverter(IRecordComposer recordComposer)
        {
            _recordComposer = recordComposer;
        }


        public DataType ConvertTypeReference(
            TypeReference typeReference, 
            IVhdlTransformationContext context)
        {
            switch (typeReference.FullName)
            {
                case "System.Boolean":
                    return ConvertPrimitive(KnownTypeCode.Boolean);
                case "System.Byte":
                    return ConvertPrimitive(KnownTypeCode.Byte);
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
                case "System.SByte":
                    return ConvertPrimitive(KnownTypeCode.SByte);
                case "System.String":
                    return ConvertPrimitive(KnownTypeCode.String);
                case "System.UInt16":
                    return ConvertPrimitive(KnownTypeCode.UInt16);
                case "System.UInt32":
                    return ConvertPrimitive(KnownTypeCode.UInt32);
                case "System.UInt64":
                    return ConvertPrimitive(KnownTypeCode.UInt64);
                case "System.Void":
                    return ConvertPrimitive(KnownTypeCode.Void);
            }

            if (typeReference.IsArray)
            {
                return CreateArrayType(ConvertTypeReference(typeReference.GetElementType(), context));
            }

            if (IsTaskTypeReference(typeReference))
            {
                if (typeReference is GenericInstanceType)
                {
                    return ConvertTypeReference(((GenericInstanceType)typeReference).GenericArguments.Single(), context); 
                }

                return SpecialTypes.Task;
            }

            return ConvertTypeDefinition(typeReference as TypeDefinition, typeReference.FullName, context);
        }

        public DataType ConvertAstType(AstType type, IVhdlTransformationContext context)
        {
            if (type is PrimitiveType) return ConvertPrimitive(((PrimitiveType)type).KnownTypeCode);
            else if (type is ComposedType composedType)
            {

                // For inner classes (member types) the BaseType will contain the actual type (in a strange way the 
                // actual type will be the BaseType of itself...).
                if (type.GetFullName() == composedType.BaseType.GetFullName())
                {
                    return ConvertAstType(composedType.BaseType, context);
                }
                else
                {
                    return ConvertComposed(composedType, context);
                }
            }
            else if (type is SimpleType) return ConvertSimple((SimpleType)type, context);

            return ConvertTypeDefinition(type.Annotation<TypeDefinition>(), type.GetFullName(), context);
        }

        public DataType ConvertAndDeclareAstType(
            AstType type, 
            IDeclarableElement declarable,
            IVhdlTransformationContext context)
        {
            var vhdlType = ConvertAstType(type, context);

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
                    return KnownDataTypes.UInt8;
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
                    return KnownDataTypes.Int8;
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

        private DataType ConvertComposed(ComposedType type, IVhdlTransformationContext context)
        {
            if (type.IsArray())
            {
                return CreateArrayType(ConvertAstType(type.BaseType, context));
            }

            // If the type is used in an array initialization and is a non-primitive type then the actual type will be 
            // the only child.
            if (type.Children.SingleOrDefault() is SimpleType)
            {
                return ConvertSimple((SimpleType)type.Children.SingleOrDefault(), context);
            }

            // If the type is used in an array initialization and is a primitive type then the actual type will be the
            // BaseType.
            if (type.BaseType is PrimitiveType)
            {
                return ConvertPrimitive(((PrimitiveType)type.BaseType).KnownTypeCode);
            }

            throw new NotSupportedException("The type " + type.ToString() + " is not supported for transforming.");
        }

        private DataType ConvertSimple(SimpleType type, IVhdlTransformationContext context)
        {
            if (type.Identifier == nameof(System.Threading.Tasks.Task) && IsTaskTypeReference(type.GetActualTypeReference()))
            {
                // Changing e.g. Task<bool> to bool. Then it will be handled later what to do with the Task.
                if (type.TypeArguments.Count == 1)
                {
                    return ConvertAstType(type.TypeArguments.Single(), context);
                }

                return SpecialTypes.Task;
            }

            return ConvertTypeDefinition(type.Annotation<TypeDefinition>(), type.GetFullName(), context);
        }

        private DataType ConvertTypeDefinition(TypeDefinition typeDefinition, string typeFullName, IVhdlTransformationContext context)
        {
            if (typeDefinition == null)
            {
                typeDefinition = context.TypeDeclarationLookupTable.Lookup(typeFullName)?.Annotation<TypeDefinition>();
            }

            if (typeDefinition == null) ExceptionHelper.ThrowDeclarationNotFoundException(typeFullName);

            if (typeDefinition.IsEnum)
            {
                return new VhdlBuilder.Representation.Declaration.Enum { Name = typeDefinition.FullName.ToExtendedVhdlId() };
            }

            if (typeDefinition.IsClass)
            {
                var typeDeclaration = context.TypeDeclarationLookupTable.Lookup(typeDefinition.FullName);

                if (typeDeclaration == null) ExceptionHelper.ThrowDeclarationNotFoundException(typeDefinition.FullName);

                return _recordComposer.CreateRecordFromType(typeDeclaration, context);
            }

            throw new NotSupportedException(
                "The type " + typeDefinition.FullName + " is not supported for transforming.");
        }


        private static DataType CreateArrayType(DataType elementType) =>
            new VhdlBuilder.Representation.Declaration.ArrayType
            {
                ElementType = elementType,
                Name = ArrayHelper.CreateArrayTypeName(elementType)
            };

        private static bool IsTaskTypeReference(TypeReference typeReference) =>
            typeReference.FullName.StartsWith(typeof(System.Threading.Tasks.Task).FullName);
    }
}
