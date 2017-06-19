using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.Transformer.Vhdl.Models
{
    public class NullableRecord : Record
    {
        public static readonly string IsNullFieldName = "IsNull".ToExtendedVhdlId();

        public IDataObject IsNullFieldReference { get; private set; }


        public NullableRecord()
        {
            var isNullField = new RecordField
            {
                DataType = KnownDataTypes.Boolean,
                Name = IsNullFieldName
            };

            IsNullFieldReference = isNullField.ToReference();
            Fields.Add(isNullField);
        }


        public static RecordFieldAccess CreateIsNullFieldAccess(IDataObject recordInstance) =>
            new RecordFieldAccess { Instance = recordInstance, FieldName = IsNullFieldName };
    }
}
