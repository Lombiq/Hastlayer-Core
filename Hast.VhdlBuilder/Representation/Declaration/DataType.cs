using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public enum DataTypeCategory
    {
        Numeric,
        Identifier, // Like in type T_STATE is (IDLE, READ, END_CYC); e.g IDLE or even boolean
        Array,
        Character,
        Unit,
        Composite
    }

    /// <summary>
    /// VHDL object data type, e.g. std_logic or std_logic_vector.
    /// </summary>
    public class DataType : INamedElement
    {
        public DataTypeCategory TypeCategory { get; set; }
        public string Name { get; set; }

        public virtual string ToVhdl()
        {
            return Name;
        }
    }
}
