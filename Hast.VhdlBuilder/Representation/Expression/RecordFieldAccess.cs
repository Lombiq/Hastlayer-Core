using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    /// <summary>
    /// A record's field's access expression, i.e. myRecord.Member.
    /// </summary>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class RecordFieldAccess : DataObjectBase
    {
        private IDataObject _instance;
        public IDataObject Instance
        {
            get { return _instance; }
            set
            {
                _instance = value;
                DataObjectKind = value.DataObjectKind;
                Name = value.Name;
            }
        }

        public string FieldName { get; set; }


        public override IDataObject ToReference()
        {
            return this;
        }

        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return vhdlGenerationOptions.ShortenName(Instance.Name) + "." + FieldName;
        }
    }
}
