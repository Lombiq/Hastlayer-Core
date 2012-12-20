using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.TypeSystem;
using VhdlBuilder;

namespace HastTranspiler.Vhdl
{
    static class PrimitiveTypeConverter
    {
        public static DataType Convert(KnownTypeCode typeCode)
        {
            switch (typeCode)
            {
                case KnownTypeCode.Array:
                    break;
                case KnownTypeCode.Attribute:
                    break;
                case KnownTypeCode.Boolean:
                    break;
                case KnownTypeCode.Byte:
                    break;
                case KnownTypeCode.Char:
                    break;
                case KnownTypeCode.DBNull:
                    break;
                case KnownTypeCode.DateTime:
                    break;
                case KnownTypeCode.Decimal:
                    break;
                case KnownTypeCode.Delegate:
                    break;
                case KnownTypeCode.Double:
                    break;
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
                    return new RangedDataType { Name = "integer", RangeMin = -32768, RangeMax = 32767 };
                case KnownTypeCode.Int32:
                    return new RangedDataType { Name = "integer", RangeMin = -2147483648, RangeMax = 2147483647 };
                case KnownTypeCode.Int64:
                    return new RangedDataType { Name = "integer", RangeMin = -2147483648, RangeMax = 2147483647 };
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
                    return new SizedDataType { Name = "string", Size = int.MaxValue };
                case KnownTypeCode.Task:
                    break;
                case KnownTypeCode.TaskOfT:
                    break;
                case KnownTypeCode.Type:
                    break;
                case KnownTypeCode.UInt16:
                    break;
                case KnownTypeCode.UInt32:
                    break;
                case KnownTypeCode.UInt64:
                    break;
                case KnownTypeCode.UIntPtr:
                    break;
                case KnownTypeCode.ValueType:
                    break;
                case KnownTypeCode.Void:
                    break;
                default:
                    break;
            }

            return null;
        }
    }
}
