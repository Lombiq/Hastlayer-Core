using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public enum DataTypeCategory
    {
        Scalar,
        Identifier, // Like in type T_STATE is (IDLE, READ, END_CYC); e.g IDLE or even boolean
        Array,
        Character,
        Unit,
        Composite
    }


    /// <summary>
    /// VHDL object data type, e.g. std_logic or std_logic_vector.
    /// </summary>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class DataType : INamedElement, IReferenceableDeclaration<DataType>
    {
        public DataTypeCategory TypeCategory { get; set; }
        public string Name { get; set; }
        public Value DefaultValue { get; set; }


        public DataType(DataType previous) : this()
        {
            TypeCategory = previous.TypeCategory;
            Name = previous.Name;
            DefaultValue = previous.DefaultValue;
        }

        public DataType()
        {
        }


        /// <summary>
        /// Generates VHDL code that can be used when the data type is referenced e.g. in a variable declaration.
        /// </summary>
        /// <remarks>
        /// This is necessary because enums are declared and used in variables differently. Note that this is a different
        /// concept from <see cref="DataObjectReference"/> which is about referencing data objects (e.g. signals), not
        /// data types.
        /// </remarks>
        public virtual DataType ToReference()
        {
            return new DataTypeReference(this, vhdlGenerationOptions => vhdlGenerationOptions.NameShortener(Name));
        }

        public virtual string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return vhdlGenerationOptions.NameShortener(Name);
        }

        /// <summary>
        /// Indicated whether this data type is among the types that can be assigned to an array as a literal inside 
        /// double quotes.
        /// </summary>
        public virtual bool IsLiteralArrayType()
        {
            return this == KnownDataTypes.String || Name == "bit_vector" || Name == "std_logic_vector";
        }

        public override bool Equals(object obj)
        {
            var otherType = obj as DataType;
            if (otherType == null) return false;
            return Name == otherType.Name && TypeCategory == otherType.TypeCategory;
        }


        public static bool operator ==(DataType a, DataType b)
        {
            // If both are null, or both are the same instance, return true (ReferenceEquals() returns true if both
            // object are null).
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Else return true if the data types match:
            return a.Equals(b);
        }

        public static bool operator !=(DataType a, DataType b)
        {
            return !(a == b);
        }
    }
}
