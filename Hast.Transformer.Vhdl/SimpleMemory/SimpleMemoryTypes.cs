using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.SimpleMemory
{
    internal static class SimpleMemoryTypes
    {
        public static readonly DataType CellIndexSignalDataType = KnownDataTypes.UnrangedInt;
        public static readonly SizedDataType CellIndexInternalSignalDataType = KnownDataTypes.Int32;
        public static readonly DataType EnableSignalsDataType = KnownDataTypes.Boolean;
        public static readonly DataType DoneSignalsDataType = KnownDataTypes.Boolean;


        public static DataType DataSignalsDataType(uint dataBusWidthBytes) => 
            new StdLogicVector { Size = (int)dataBusWidthBytes * 8 };
    }
}
