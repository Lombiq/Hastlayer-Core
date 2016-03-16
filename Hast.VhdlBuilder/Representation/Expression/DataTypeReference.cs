using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    internal class DataTypeReference : DataType
    {
        private readonly Func<IVhdlGenerationOptions, string> _vhdlGenerator;


        public DataTypeReference(DataType dataType, Func<IVhdlGenerationOptions, string> vhdlGenerator) : base(dataType)
        {
            _vhdlGenerator = vhdlGenerator;
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return _vhdlGenerator(vhdlGenerationOptions);
        }
    }
}
