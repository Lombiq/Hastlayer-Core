using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public enum DataObjectKind
    {
        Constant,
        Variable,
        Signal,
        File
    }


    public interface IDataObject : INamedElement
    {
        DataObjectKind DataObjectKind { get; set; }
    }
}
