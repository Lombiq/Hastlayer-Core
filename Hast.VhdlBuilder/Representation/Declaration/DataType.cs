using System.Diagnostics;

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
    [DebuggerDisplay("{ToVhdl()}")]
    public class DataType : INamedElement
    {
        public DataTypeCategory TypeCategory { get; set; }
        public string Name { get; set; }


        public virtual string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return Name;
        }

        public virtual string ToReferenceVhdl()
        {
            return Name;
        }


        public static bool operator ==(DataType a, DataType b)
        {
            // If both are null, or both are the same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Else return true if names match:
            return a.Name == b.Name;
        }

        public static bool operator !=(DataType a, DataType b)
        {
            return !(a == b);
        }
    }
}
