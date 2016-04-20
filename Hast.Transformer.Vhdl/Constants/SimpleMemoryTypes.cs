using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Constants
{
    internal static class SimpleMemoryTypes
    {
        public static readonly DataType DataSignalsDataType = new StdLogicVector { Size = 32 };
        public static readonly DataType CellIndexSignalDataType = KnownDataTypes.UnrangedInt;
        public static readonly DataType EnableSignalsDataType = KnownDataTypes.Boolean;
        public static readonly DataType DoneSignalsDataType = KnownDataTypes.Boolean;
    }
}
