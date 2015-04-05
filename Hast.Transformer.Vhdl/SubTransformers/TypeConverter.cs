using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Hast.VhdlBuilder.Representation.Declaration;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface ITypeConverter : IDependency
    {
        DataType Convert(AstType type);
        DataType ConvertAndDeclare(AstType type, IDeclarableElement declarable);
    }


    public class TypeConverter : ITypeConverter
    {
        public DataType Convert(AstType type)
        {
            if (type is PrimitiveType) return ConvertPrimitive((type as PrimitiveType).KnownTypeCode);
            else if (type is ComposedType) return ConvertComposed((ComposedType)type);
            //else if (type is SimpleType) return ConvertSimple((SimpleType)type);

            throw new NotSupportedException("This type is not supported for transforming.");
        }

        public DataType ConvertAndDeclare(AstType type, IDeclarableElement declarable)
        {
            var vhdlType = Convert(type);

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
                    return DataTypes.Boolean;
                case KnownTypeCode.Byte:
                    break;
                case KnownTypeCode.Char:
                    return DataTypes.Character;
                case KnownTypeCode.DBNull:
                    break;
                case KnownTypeCode.DateTime:
                    break;
                case KnownTypeCode.Decimal:
                    break;
                case KnownTypeCode.Delegate:
                    break;
                case KnownTypeCode.Double:
                    return DataTypes.Real;
                case KnownTypeCode.Enum:
                    return DataTypes.Enum;
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
                    return DataTypes.Int16;
                case KnownTypeCode.Int32:
                    // The lower barrier for VHDL integers is one shorter...
                    return DataTypes.Int32;
                case KnownTypeCode.Int64:
                    break;
                case KnownTypeCode.IntPtr:
                    break;
                case KnownTypeCode.MulticastDelegate:
                    break;
                case KnownTypeCode.None:
                    break;
                case KnownTypeCode.NullableOfT:
                    break;
                case KnownTypeCode.Object:
                    break;
                case KnownTypeCode.SByte:
                    break;
                case KnownTypeCode.Single:
                    break;
                case KnownTypeCode.String:
                    return DataTypes.String;
                case KnownTypeCode.Task:
                    break;
                case KnownTypeCode.TaskOfT:
                    break;
                case KnownTypeCode.Type:
                    break;
                case KnownTypeCode.UInt16:
                    return DataTypes.Natural;
                case KnownTypeCode.UInt32:
                    return DataTypes.Natural;
                case KnownTypeCode.UInt64:
                    break;
                case KnownTypeCode.UIntPtr:
                    break;
                case KnownTypeCode.ValueType:
                    break;
                case KnownTypeCode.Void:
                    return new DataType { Name = "void" };
            }

            return null;
        }

        private DataType ConvertComposed(ComposedType type)
        {
            if (type.ArraySpecifiers.Count != 0)
            {
                var storedType = Convert(type.BaseType);
                return new Hast.VhdlBuilder.Representation.Declaration.Array { StoredType = storedType, Name = storedType.Name + "_array" };
            }

            return null;
        }

        private DataType ConvertSimple(SimpleType type)
        {
            throw new NotImplementedException();
        }
    }
}
