using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

// The same namespace as KnownDataTypes so common types can be checked simply.
namespace Hast.VhdlBuilder.Representation.Declaration
{
    public static class SpecialTypes
    {
        public static DataType Task = new DataType { TypeCategory = DataTypeCategory.Identifier, Name = "Task" };
    }
}
