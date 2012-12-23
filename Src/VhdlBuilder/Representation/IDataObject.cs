using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
{
    public interface IDataObject : IVhdlElement
    {
        string Name { get; set; }
        DataType DataType { get; set; }
    }
}
