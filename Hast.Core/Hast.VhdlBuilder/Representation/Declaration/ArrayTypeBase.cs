using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public abstract class ArrayTypeBase : DataType
    {
        public DataType ElementType { get; set; }

        private Value _defaultValue;
        public override Value DefaultValue
        {
            get
            {
                if (_defaultValue == null && ElementType != null && ElementType.DefaultValue != null)
                {
                    _defaultValue = CreateDefaultInitialization(this, ElementType);
                }

                return _defaultValue;
            }
            set { _defaultValue = value; }
        }


        public ArrayTypeBase()
        {
            TypeCategory = DataTypeCategory.Array;
        }


        public static Value CreateDefaultInitialization(DataType arrayInstantiationType, DataType elementType) =>
            new Value
            {
                DataType = arrayInstantiationType,
                Content = "others => " + elementType.DefaultValue.ToVhdl()
            };
    }
}
