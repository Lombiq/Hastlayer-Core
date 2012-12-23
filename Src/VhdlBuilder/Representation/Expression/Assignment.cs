using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder.Representation.Declaration;

namespace VhdlBuilder.Representation.Expression
{
    public class Assignment : IVhdlElement
    {
        public DataObjectBase DataObject { get; set; }
        public string Expression { get; set; }

        public string ToVhdl()
        {
            return
                DataObject.Name.ToVhdlId() +
                (DataObject.ObjectType == ObjectType.Variable ? " := " : " <= ") +
                Expression +
                ";";
        }
    }
}
