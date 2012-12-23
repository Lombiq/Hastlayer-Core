using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation.Declaration
{
    public class Variable : DataObject
    {
        public Variable()
        {
            ObjectType = ObjectType.Variable;
        }
    }
}
