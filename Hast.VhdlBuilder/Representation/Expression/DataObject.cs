using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    public class DataObject : DataObjectBase
    {
        public override string ToVhdl()
        {
            return Name.ToVhdlId();
        }
    }
}
