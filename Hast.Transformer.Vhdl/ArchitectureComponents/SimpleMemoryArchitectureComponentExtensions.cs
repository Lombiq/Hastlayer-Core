using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Constants;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    /// <summary>
    /// Handles intermediay signals for using SimpleMemory ports. Such signals are only needed for Out or InOut ports,
    /// In ports can be simply read from multiple places; so intermediary signals are only needed for the CellIndex,
    /// DataOut, ReadEnable and WriteEnable ports.
    /// </summary>
    public static class SimpleMemoryArchitectureComponentExtensions
    {
        public static bool AreSimpleMemorySignalsAdded(this IArchitectureComponent component)
        {
            // If there is a signal for CellIndex then all the others should be added too.
            var signalName = component.CreateSimpleMemorySignalName(SimpleMemoryPortNames.CellIndex);
            return component.Signals.Any(signal => signal.Name == signalName);
        }

        public static void AddSimpleMemorySignalsIfNew(this IArchitectureComponent component)
        {
            if (component.AreSimpleMemorySignalsAdded()) return;


            component.Signals.Add(new Signal
                {
                    DataType = SimpleMemoryTypes.CellIndexSignalDataType,
                    Name = component.CreateSimpleMemorySignalName(SimpleMemoryPortNames.CellIndex)
                });
            component.Signals.Add(new Signal
                {
                    DataType = SimpleMemoryTypes.DataSignalsDataType,
                    Name = component.CreateSimpleMemorySignalName(SimpleMemoryPortNames.DataOut)
                });
            component.Signals.Add(new Signal
                {
                    DataType = SimpleMemoryTypes.EnableSignalsDataType,
                    Name = component.CreateSimpleMemorySignalName(SimpleMemoryPortNames.ReadEnable)
                });
            component.Signals.Add(new Signal
                {
                    DataType = SimpleMemoryTypes.EnableSignalsDataType,
                    Name = component.CreateSimpleMemorySignalName(SimpleMemoryPortNames.WriteEnable)
                });
        }

        public static DataObjectReference CreateSimpleMemoryCellIndexSignalReference(this IArchitectureComponent component)
        {
            return component.CreateSimpleMemorySignalReference(SimpleMemoryPortNames.CellIndex);
        }

        public static DataObjectReference CreateSimpleMemoryDataOutSignalReference(this IArchitectureComponent component)
        {
            return component.CreateSimpleMemorySignalReference(SimpleMemoryPortNames.DataOut);
        }

        public static DataObjectReference CreateSimpleMemoryReadEnableSignalReference(this IArchitectureComponent component)
        {
            return component.CreateSimpleMemorySignalReference(SimpleMemoryPortNames.ReadEnable);
        }

        public static DataObjectReference CreateSimpleMemoryWriteEnableSignalReference(this IArchitectureComponent component)
        {
            return component.CreateSimpleMemorySignalReference(SimpleMemoryPortNames.WriteEnable);
        }


        private static DataObjectReference CreateSimpleMemorySignalReference(
            this IArchitectureComponent component, 
            string simpleMemoryPortName)
        {
            return component.CreateSimpleMemorySignalName(simpleMemoryPortName).ToVhdlSignalReference();
        }

        private static string CreateSimpleMemorySignalName(
            this IArchitectureComponent component, 
            string simpleMemoryPortName)
        {
            return component.CreatePrefixedSegmentedObjectName("SimpleMemory", simpleMemoryPortName).ToExtendedVhdlId();
        }
    }
}
