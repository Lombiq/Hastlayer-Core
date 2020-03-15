using Hast.VhdlBuilder.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Represents a Xilinx XDC constraints file.
    /// </summary>
    /// <remarks>
    /// See <see cref="Expression.MultiCycleSdcStatementsAttributeExpression"/> for something similar for Quartus Prime.
    /// </remarks>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class XdcFile : IVhdlElement
    {
        public List<IVhdlElement> Lines { get; set; } = new List<IVhdlElement>();


        public void AddPath(IDataObject pathReference, int clockCycles, bool isHierarchical)
        {
            Lines.Add(new XdcPath
            {
                PathReference = pathReference,
                ClockCycles = clockCycles,
                Type = "setup",
                IsHierarchical = isHierarchical
            });

            Lines.Add(new XdcPath
            {
                PathReference = pathReference,
                ClockCycles = clockCycles - 1,
                Type = "hold",
                IsHierarchical = isHierarchical
            });
        }

        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            string.Join(Environment.NewLine, Lines.Select(line => line.ToVhdl(vhdlGenerationOptions)));


        /// <summary>
        /// Represents a path constraint declarations like:
        /// set_multicycle_path 8 -setup -to [get_cells -hierarchical {*PrimeCalculator::IsPrimeNumber(SimpleMemory).0.binaryOperationResult.2*}]
        /// </summary>
        private class XdcPath : IVhdlElement
        {
            public IDataObject PathReference { get; set; }
            public int ClockCycles { get; set; }
            public string Type { get; set; }
            public bool IsHierarchical { get; set; }


            public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
                    "set_multicycle_path " + ClockCycles + " -" + Type + " -to [get_cells " +
                    (IsHierarchical ? "-hierarchical " : string.Empty) + "{*" +
                    // The config should contain the path's name without backslashes even if the original name is an
                    // extended identifier.
                    PathReference.ToVhdl(vhdlGenerationOptions).TrimExtendedVhdlIdDelimiters() +
                    "*}]";
        }
    }
}
