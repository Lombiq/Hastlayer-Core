using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using VhdlBuilder.Representation;

namespace HastTranspiler.Vhdl.SubTranspilers
{
    public class TypeConverter
    {
        public DataType Convert(AstType type)
        {
            if (type is PrimitiveType) return ConvertPrimitive((type as PrimitiveType).KnownTypeCode);
            return null;
        }

        public DataType ConvertPrimitive(KnownTypeCode typeCode)
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
                default:
                    break;
            }

            return null;
        }
    }
}
