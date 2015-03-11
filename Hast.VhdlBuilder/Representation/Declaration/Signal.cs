using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public class Signal : DataObject
    {
        public Signal()
        {
            ObjectType = ObjectType.Signal;
        }
    }
}
