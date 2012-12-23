using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation.Declaration
{
    public enum ObjectType
    {
        Constant,
        Variable,
        Signal,
        File
    }

    public interface IDataObject : IVhdlElement
    {
        ObjectType ObjectType { get; set; }
        string Name { get; set; }
        DataType DataType { get; set; }
    }
}
