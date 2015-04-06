using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public abstract class DataObjectBase : IDataObject
    {
        public DataObjectKind DataObjectKind { get; set; }
        public string Name { get; set; }


        public abstract string ToVhdl();
    }
}
