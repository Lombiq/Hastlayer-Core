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
        public static readonly DataType DataPortsDataType = new StdLogicVector { Size = 32 };
        public static readonly DataType CellIndexPortDataType = KnownDataTypes.UnrangedInt;
        public static readonly DataType EnablePortsDataType = KnownDataTypes.StdLogic;
        public static readonly DataType DonePortsDataType = KnownDataTypes.StdLogic;
    }
}
