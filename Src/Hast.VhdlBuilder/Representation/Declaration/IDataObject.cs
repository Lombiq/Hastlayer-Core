using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public enum ObjectType
    {
        Constant,
        Variable,
        Signal,
        File
    }

    public interface IDataObject : INamedElement
    {
        ObjectType ObjectType { get; set; }
        DataType DataType { get; set; }
    }
}
