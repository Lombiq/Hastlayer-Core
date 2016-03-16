using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    /// <summary>
    /// An array element access expression, i.e. array(index).
    /// </summary>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class ArrayElementAccess : DataObjectBase
    {
        private IDataObject _array;
        public IDataObject Array
        {
            get { return _array; }
            set
            {
                _array = value;
                DataObjectKind = value.DataObjectKind;
                Name = value.Name;
            }
        }
        
        public IVhdlElement IndexExpression { get; set; }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                vhdlGenerationOptions.ShortenName(Array.Name) + "(" + IndexExpression.ToVhdl(vhdlGenerationOptions) + ")";
        }
    }
}
