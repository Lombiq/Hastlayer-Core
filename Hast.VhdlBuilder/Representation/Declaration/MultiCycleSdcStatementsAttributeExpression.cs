﻿using Hast.VhdlBuilder.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Represents an attribute expression containing SDC_STATEMENT that define multi-cycle paths.
    /// </summary>
    /// <example>
    /// E.g. represents the expression part of the below attribute specification.
    /// 	attribute altera_attribute of Imp: architecture is "-name SDC_STATEMENT ""set_multicycle_path 8 -setup -to {*PrimeCalculator::IsPrimeNumber(SimpleMemory).0._StateMachine:PrimeCalculator::IsPrimeNumber(SimpleMemory).0.binaryOperationResult.2[*]}"";-name SDC_STATEMENT ""set_multicycle_path 8 -hold  -to {*PrimeCalculator::IsPrimeNumber(SimpleMemory).0._StateMachine:PrimeCalculator::IsPrimeNumber(SimpleMemory).0.binaryOperationResult.2[*]}""";
    /// </example>
    /// <remarks>
    /// See <see cref="XdcFile"/> for something similar for Xilinx.
    /// </remarks>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class MultiCycleSdcStatementsAttributeExpression : IVhdlElement
    {
        private readonly List<SdcStatement> _paths = new List<SdcStatement>();


        public void AddPath(string parentName, IDataObject pathReference, int clockCycles)
        {
            _paths.Add(new SdcStatement
            {
                ParentName = parentName,
                PathReference = pathReference,
                ClockCycles = clockCycles,
                Type = "setup"
            });

            _paths.Add(new SdcStatement
            {
                ParentName = parentName,
                PathReference = pathReference,
                ClockCycles = clockCycles - 1,
                Type = "hold"
            });
        }

        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
            Terminated.Terminate(
                "\"" +
                string.Join(";", _paths.Select(statement => statement.ToVhdl(vhdlGenerationOptions))) +
                "\"", vhdlGenerationOptions);


        private class SdcStatement : IVhdlElement
        {
            public string ParentName { get; set; }
            public IDataObject PathReference { get; set; }
            public int ClockCycles { get; set; }
            public string Type { get; set; }


            public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) =>
                    "-name SDC_STATEMENT \"\"set_multicycle_path " + ClockCycles + " -" + Type + " -to {*" +
                    // The config should contain the path's name without backslashes even if the original name is an
                    // extended identifier. Spaces need to be escaped with a slash.
                    (string.IsNullOrEmpty(ParentName) ?
                        string.Empty :
                        vhdlGenerationOptions.NameShortener(ParentName.TrimExtendedVhdlIdDelimiters().Replace(" ", "\\ ")) + ":") +
                    PathReference.ToVhdl(vhdlGenerationOptions).TrimExtendedVhdlIdDelimiters().Replace(" ", "\\ ") +
                    "[*]}\"\"";
        }
    }
}
