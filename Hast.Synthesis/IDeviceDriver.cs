using Hast.Synthesis.Abstractions;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Synthesis
{
    /// <summary>
    /// Provides FPGA-specific implementations.
    /// </summary>
    /// <remarks>
    /// <para>Separated the manifest-providing part into <see cref="IDeviceManifestProvider"/> so that can be available in
    /// the Client flavor too but keeping the rest of the driver implementation only part of Hast.Core.</para>
    /// </remarks>
    public interface IDeviceDriver : IDeviceManifestProvider
    {
        /// <summary>
        /// Returns the number of cycles required to perform <paramref name="expression"/> that has 2 operands.
        /// </summary>
        decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression, int operandSizeBits, bool isSigned);

        /// <summary>
        /// Returns the number of cycles required to perform <paramref name="expression"/> that has 1 operand.
        /// </summary>
        decimal GetClockCyclesNeededForUnaryOperation(UnaryOperatorExpression expression, int operandSizeBits, bool isSigned);
    }
}
