using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public abstract class TypedDataObjectBase : DataObjectBase, ITypedDataObject
    {
        public DataType DataType { get; set; }


        public DataObjectReference ToReference()
        {
            return new DataObjectReference { DataObjectKind = DataObjectKind, Name = Name.ToExtendedVhdlId() };
        }
    }
}
