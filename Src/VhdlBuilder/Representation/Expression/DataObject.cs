using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder.Representation.Declaration;

namespace VhdlBuilder.Representation.Expression
{
    public class DataObject : DataObjectBase
    {
        public override string ToVhdl()
        {
            return Name.ToVhdlId();
        }
    }
}
