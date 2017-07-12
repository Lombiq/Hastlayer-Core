using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Synthesis.Abstractions;
using Hast.Synthesis.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Synthesis
{
    /// <summary>
    /// Provides FPGA-specific implementations.
    /// </summary>
    /// <remarks>
    /// Separated the manifest-providing part into <see cref="IDeviceManifestProvider"/> so that can be available in 
    /// the Client flavor too but keeping the rest of the driver implementation only part of Hast.Core.
    /// </remarks>
    public interface IDeviceDriver : IDeviceManifestProvider
    {
        decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression, int operandSizeBits, bool isSigned);
        decimal GetClockCyclesNeededForUnaryOperation(UnaryOperatorExpression expression, int operandSizeBits, bool isSigned);
    }
}
