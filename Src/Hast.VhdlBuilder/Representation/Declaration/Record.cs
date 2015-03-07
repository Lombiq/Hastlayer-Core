using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public class Record : DataType
    {
        public List<DataObject> Members { get; set; }

        public Record()
        {
            Members = new List<DataObject>();
        }


        public override string ToVhdl()
        {
            return
                "type " +
                Name.ToVhdlId() +
                " is record " +
                Members.ToVhdl() +
                " end record;";
        }
    }
}
