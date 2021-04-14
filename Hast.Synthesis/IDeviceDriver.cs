using Hast.Synthesis.Abstractions;
using Hast.Synthesis.Helpers;
using Hast.Synthesis.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;

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
        ITimingReport TimingReport { get; }

        decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression, int operandSizeBits, bool isSigned) =>
            DeviceDriverHelper.ComputeClockCyclesForBinaryOperation(DeviceManifest, TimingReport, expression, operandSizeBits, isSigned);

        decimal GetClockCyclesNeededForUnaryOperation(UnaryOperatorExpression expression, int operandSizeBits, bool isSigned) =>
            DeviceDriverHelper.ComputeClockCyclesForUnaryOperation(DeviceManifest, TimingReport, expression, operandSizeBits, isSigned);
    }
}
