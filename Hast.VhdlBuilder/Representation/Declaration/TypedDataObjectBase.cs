using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public abstract class TypedDataObjectBase : DataObjectBase, ITypedDataObject
    {
        public DataType DataType { get; set; }


        public DataObjectReference ToReference()
        {
            return new DataObjectReference { DataObjectKind = DataObjectKind, Name = Name };
        }
    }
}
