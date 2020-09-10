using Hast.Transformer.Abstractions.SimpleMemory;
// ReSharper disable RedundantCast

namespace Hast.TestInputs.Dynamic
{
    /// <summary>
    /// Test class to use all binary and unary operator expression variations. Needed to check whether proper type
    /// conversions are implemented.
    /// </summary>
    /// <remarks>
    /// Note that all the cases for u/long won't fit on the Nexys A7 so we need to split them. Ideally all the cases
    /// would fit into a single design though. Hardware resource usage for each of these tests is as following:
    /// - ByteBinaryOperatorExpressionVariations: 52%
    /// - SbyteBinaryOperatorExpressionVariations: 64%
    /// - ShortBinaryOperatorExpressionVariations: 64%
    /// - UshortBinaryOperatorExpressionVariations: 55%
    /// - IntBinaryOperatorExpressionVariations: 63%
    /// - UintBinaryOperatorExpressionVariations: 82%
    /// - LongBinaryOperatorExpressionVariationsLow: 71%
    /// - LongBinaryOperatorExpressionVariationsHigh: 53%
    /// - UlongBinaryOperatorExpressionVariationsLow: 33%
    /// - UlongBinaryOperatorExpressionVariationsHigh: 33%
    /// - AllUnaryOperatorExpressionVariations: 33%
    ///
    /// While using the 8-number SaveResult() method actually slightly increases resource usage synthesis time is
    /// greatly reduced (as opposed to calling the single-number SaveResult() for every number).
    /// <see cref="BinaryAndUnaryOperatorExpressionCasesGenerator"/> can be used to generate these cases.
    /// </remarks>
    public class BinaryAndUnaryOperatorExpressionCases : DynamicTestInputBase
    {
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand

        public virtual void ByteBinaryOperatorExpressionVariations(SimpleMemory memory)
        {
            var input = memory.ReadInt32(0);
            var byteLeft = (byte)input;

            var byteRight = (byte)input;
            SaveResult(memory, 0, (long)(byteLeft << byteRight));
            SaveResult(memory, 2, (long)(byteLeft >> byteRight));
            SaveResult(
                memory,
                4,
                (long)(byteLeft + byteRight), // 4
                (long)(byteLeft - byteRight), // 6
                (long)(byteLeft * byteRight), // 8
                (long)(byteLeft / byteRight), // 10
                (long)(byteLeft % byteRight), // 12
                (long)(byteLeft & byteRight), // 14
                (long)(byteLeft | byteRight), // 16
                (long)(byteLeft ^ byteRight)); // 18

            var sbyteRight = (sbyte)input;
            SaveResult(memory, 20, (long)(byteLeft << sbyteRight));
            SaveResult(memory, 22, (long)(byteLeft >> sbyteRight));
            SaveResult(
                memory,
                24,
                (long)(byteLeft + sbyteRight), // 24
                (long)(byteLeft - sbyteRight), // 26
                (long)(byteLeft * sbyteRight), // 28
                (long)(byteLeft / sbyteRight), // 30
                (long)(byteLeft % sbyteRight), // 32
                (long)(byteLeft & sbyteRight), // 34
                (long)(byteLeft | sbyteRight), // 36
                (long)(byteLeft ^ sbyteRight)); // 38

            var shortRight = (short)input;
            SaveResult(memory, 40, (long)(byteLeft << shortRight));
            SaveResult(memory, 42, (long)(byteLeft >> shortRight));
            SaveResult(
                memory,
                44,
                (long)(byteLeft + shortRight), // 44
                (long)(byteLeft - shortRight), // 46
                (long)(byteLeft * shortRight), // 48
                (long)(byteLeft / shortRight), // 50
                (long)(byteLeft % shortRight), // 52
                (long)(byteLeft & shortRight), // 54
                (long)(byteLeft | shortRight), // 56
                (long)(byteLeft ^ shortRight)); // 58

            var ushortRight = (ushort)input;
            SaveResult(memory, 60, (long)(byteLeft << ushortRight));
            SaveResult(memory, 62, (long)(byteLeft >> ushortRight));
            SaveResult(
                memory,
                64,
                (long)(byteLeft + ushortRight), // 64
                (long)(byteLeft - ushortRight), // 66
                (long)(byteLeft * ushortRight), // 68
                (long)(byteLeft / ushortRight), // 70
                (long)(byteLeft % ushortRight), // 72
                (long)(byteLeft & ushortRight), // 74
                (long)(byteLeft | ushortRight), // 76
                (long)(byteLeft ^ ushortRight)); // 78

            var intRight = input;
            SaveResult(memory, 80, (long)(byteLeft << intRight));
            SaveResult(memory, 82, (long)(byteLeft >> intRight));
            SaveResult(
                memory,
                84,
                (long)(byteLeft + intRight), // 84
                (long)(byteLeft - intRight), // 86
                (long)(byteLeft * intRight), // 88
                (long)(byteLeft / intRight), // 90
                (long)(byteLeft % intRight), // 92
                (long)(byteLeft & intRight), // 94
                (long)(byteLeft | intRight), // 96
                (long)(byteLeft ^ intRight)); // 98

            var uintRight = (uint)input;
            SaveResult(memory, 100, (long)(byteLeft << (int)uintRight));
            SaveResult(memory, 102, (long)(byteLeft >> (int)uintRight));
            SaveResult(
                memory,
                104,
                (long)(byteLeft + uintRight), // 104
                (long)(byteLeft - uintRight), // 106
                (long)(byteLeft * uintRight), // 108
                (long)(byteLeft / uintRight), // 110
                (long)(byteLeft % uintRight), // 112
                (long)(byteLeft & uintRight), // 114
                (long)(byteLeft | uintRight), // 116
                (long)(byteLeft ^ uintRight)); // 118

            var longRight = (long)input;
            SaveResult(memory, 120, (long)(byteLeft << (int)longRight));
            SaveResult(memory, 122, (long)(byteLeft >> (int)longRight));
            SaveResult(
                memory,
                124,
                (long)(byteLeft + longRight), // 124
                (long)(byteLeft - longRight), // 126
                (long)(byteLeft * longRight), // 128
                (long)(byteLeft / longRight), // 130
                (long)(byteLeft % longRight), // 132
                (long)(byteLeft & longRight), // 134
                (long)(byteLeft | longRight), // 136
                (long)(byteLeft ^ longRight)); // 138

            var ulongRight = (ulong)input;
            SaveResult(memory, 140, (long)(byteLeft << (int)ulongRight));
            SaveResult(memory, 142, (long)(byteLeft >> (int)ulongRight));
            SaveResult(
                memory,
                144,
                (long)(byteLeft + ulongRight), // 144
                (long)(byteLeft - ulongRight), // 146
                (long)(byteLeft * ulongRight), // 148
                (long)(byteLeft / ulongRight), // 150
                (long)(byteLeft % ulongRight), // 152
                (long)(byteLeft & ulongRight), // 154
                (long)(byteLeft | ulongRight), // 156
                (long)(byteLeft ^ ulongRight)); // 158
        }

        public virtual void SbyteBinaryOperatorExpressionVariations(SimpleMemory memory)
        {
            var input = memory.ReadInt32(0);
            var sbyteLeft = (sbyte)input;

            var byteRight = (byte)input;
            SaveResult(memory, 0, (long)(sbyteLeft << byteRight));
            SaveResult(memory, 2, (long)(sbyteLeft >> byteRight));
            SaveResult(
                memory,
                4,
                (long)(sbyteLeft + byteRight), // 4
                (long)(sbyteLeft - byteRight), // 6
                (long)(sbyteLeft * byteRight), // 8
                (long)(sbyteLeft / byteRight), // 10
                (long)(sbyteLeft % byteRight), // 12
                (long)(sbyteLeft & byteRight), // 14
                (long)(sbyteLeft | byteRight), // 16
                (long)(sbyteLeft ^ byteRight)); // 18

            var sbyteRight = (sbyte)input;
            SaveResult(memory, 20, (long)(sbyteLeft << sbyteRight));
            SaveResult(memory, 22, (long)(sbyteLeft >> sbyteRight));
            SaveResult(
                memory,
                24,
                (long)(sbyteLeft + sbyteRight), // 24
                (long)(sbyteLeft - sbyteRight), // 26
                (long)(sbyteLeft * sbyteRight), // 28
                (long)(sbyteLeft / sbyteRight), // 30
                (long)(sbyteLeft % sbyteRight), // 32
                (long)(sbyteLeft & sbyteRight), // 34
                (long)(sbyteLeft | sbyteRight), // 36
                (long)(sbyteLeft ^ sbyteRight)); // 38

            var shortRight = (short)input;
            SaveResult(memory, 40, (long)(sbyteLeft << shortRight));
            SaveResult(memory, 42, (long)(sbyteLeft >> shortRight));
            SaveResult(
                memory,
                44,
                (long)(sbyteLeft + shortRight), // 44
                (long)(sbyteLeft - shortRight), // 46
                (long)(sbyteLeft * shortRight), // 48
                (long)(sbyteLeft / shortRight), // 50
                (long)(sbyteLeft % shortRight), // 52
                (long)(sbyteLeft & shortRight), // 54
                (long)(sbyteLeft | shortRight), // 56
                (long)(sbyteLeft ^ shortRight)); // 58

            var ushortRight = (ushort)input;
            SaveResult(memory, 60, (long)(sbyteLeft << ushortRight));
            SaveResult(memory, 62, (long)(sbyteLeft >> ushortRight));
            SaveResult(
                memory,
                64,
                (long)(sbyteLeft + ushortRight), // 64
                (long)(sbyteLeft - ushortRight), // 66
                (long)(sbyteLeft * ushortRight), // 68
                (long)(sbyteLeft / ushortRight), // 70
                (long)(sbyteLeft % ushortRight), // 72
                (long)(sbyteLeft & ushortRight), // 74
                (long)(sbyteLeft | ushortRight), // 76
                (long)(sbyteLeft ^ ushortRight)); // 78

            var intRight = input;
            SaveResult(memory, 80, (long)(sbyteLeft << intRight));
            SaveResult(memory, 82, (long)(sbyteLeft >> intRight));
            SaveResult(
                memory,
                84,
                (long)(sbyteLeft + intRight), // 84
                (long)(sbyteLeft - intRight), // 86
                (long)(sbyteLeft * intRight), // 88
                (long)(sbyteLeft / intRight), // 90
                (long)(sbyteLeft % intRight), // 92
                (long)(sbyteLeft & intRight), // 94
                (long)(sbyteLeft | intRight), // 96
                (long)(sbyteLeft ^ intRight)); // 98

            var uintRight = (uint)input;
            SaveResult(memory, 100, (long)(sbyteLeft << (int)uintRight));
            SaveResult(memory, 102, (long)(sbyteLeft >> (int)uintRight));
            SaveResult(
                memory,
                104,
                (long)(sbyteLeft + uintRight), // 104
                (long)(sbyteLeft - uintRight), // 106
                (long)(sbyteLeft * uintRight), // 108
                (long)(sbyteLeft / uintRight), // 110
                (long)(sbyteLeft % uintRight), // 112
                (long)(sbyteLeft & uintRight), // 114
                (long)(sbyteLeft | uintRight), // 116
                (long)(sbyteLeft ^ uintRight)); // 118

            var longRight = (long)input;
            SaveResult(memory, 120, (long)(sbyteLeft << (int)longRight));
            SaveResult(memory, 122, (long)(sbyteLeft >> (int)longRight));
            SaveResult(
                memory,
                124,
                (long)(sbyteLeft + longRight), // 124
                (long)(sbyteLeft - longRight), // 126
                (long)(sbyteLeft * longRight), // 128
                (long)(sbyteLeft / longRight), // 130
                (long)(sbyteLeft % longRight), // 132
                (long)(sbyteLeft & longRight), // 134
                (long)(sbyteLeft | longRight), // 136
                (long)(sbyteLeft ^ longRight)); // 138

            var ulongRight = (ulong)input;
            SaveResult(memory, 140, (long)(sbyteLeft << (int)ulongRight));
            SaveResult(memory, 142, (long)(sbyteLeft >> (int)ulongRight));
        }

        public virtual void ShortBinaryOperatorExpressionVariations(SimpleMemory memory)
        {
            var input = memory.ReadInt32(0);
            var shortLeft = (short)input;

            var byteRight = (byte)input;
            SaveResult(memory, 0, (long)(shortLeft << byteRight));
            SaveResult(memory, 2, (long)(shortLeft >> byteRight));
            SaveResult(
                memory,
                4,
                (long)(shortLeft + byteRight), // 4
                (long)(shortLeft - byteRight), // 6
                (long)(shortLeft * byteRight), // 8
                (long)(shortLeft / byteRight), // 10
                (long)(shortLeft % byteRight), // 12
                (long)(shortLeft & byteRight), // 14
                (long)(shortLeft | byteRight), // 16
                (long)(shortLeft ^ byteRight)); // 18

            var sbyteRight = (sbyte)input;
            SaveResult(memory, 20, (long)(shortLeft << sbyteRight));
            SaveResult(memory, 22, (long)(shortLeft >> sbyteRight));
            SaveResult(
                memory,
                24,
                (long)(shortLeft + sbyteRight), // 24
                (long)(shortLeft - sbyteRight), // 26
                (long)(shortLeft * sbyteRight), // 28
                (long)(shortLeft / sbyteRight), // 30
                (long)(shortLeft % sbyteRight), // 32
                (long)(shortLeft & sbyteRight), // 34
                (long)(shortLeft | sbyteRight), // 36
                (long)(shortLeft ^ sbyteRight)); // 38

            var shortRight = (short)input;
            SaveResult(memory, 40, (long)(shortLeft << shortRight));
            SaveResult(memory, 42, (long)(shortLeft >> shortRight));
            SaveResult(
                memory,
                44,
                (long)(shortLeft + shortRight), // 44
                (long)(shortLeft - shortRight), // 46
                (long)(shortLeft * shortRight), // 48
                (long)(shortLeft / shortRight), // 50
                (long)(shortLeft % shortRight), // 52
                (long)(shortLeft & shortRight), // 54
                (long)(shortLeft | shortRight), // 56
                (long)(shortLeft ^ shortRight)); // 58

            var ushortRight = (ushort)input;
            SaveResult(memory, 60, (long)(shortLeft << ushortRight));
            SaveResult(memory, 62, (long)(shortLeft >> ushortRight));
            SaveResult(
                memory,
                64,
                (long)(shortLeft + ushortRight), // 64
                (long)(shortLeft - ushortRight), // 66
                (long)(shortLeft * ushortRight), // 68
                (long)(shortLeft / ushortRight), // 70
                (long)(shortLeft % ushortRight), // 72
                (long)(shortLeft & ushortRight), // 74
                (long)(shortLeft | ushortRight), // 76
                (long)(shortLeft ^ ushortRight)); // 78

            var intRight = input;
            SaveResult(memory, 80, (long)(shortLeft << intRight));
            SaveResult(memory, 82, (long)(shortLeft >> intRight));
            SaveResult(
                memory,
                84,
                (long)(shortLeft + intRight), // 84
                (long)(shortLeft - intRight), // 86
                (long)(shortLeft * intRight), // 88
                (long)(shortLeft / intRight), // 90
                (long)(shortLeft % intRight), // 92
                (long)(shortLeft & intRight), // 94
                (long)(shortLeft | intRight), // 96
                (long)(shortLeft ^ intRight)); // 98

            var uintRight = (uint)input;
            SaveResult(memory, 100, (long)(shortLeft << (int)uintRight));
            SaveResult(memory, 102, (long)(shortLeft >> (int)uintRight));
            SaveResult(
                memory,
                104,
                (long)(shortLeft + uintRight), // 104
                (long)(shortLeft - uintRight), // 106
                (long)(shortLeft * uintRight), // 108
                (long)(shortLeft / uintRight), // 110
                (long)(shortLeft % uintRight), // 112
                (long)(shortLeft & uintRight), // 114
                (long)(shortLeft | uintRight), // 116
                (long)(shortLeft ^ uintRight)); // 118

            var longRight = (long)input;
            SaveResult(memory, 120, (long)(shortLeft << (int)longRight));
            SaveResult(memory, 122, (long)(shortLeft >> (int)longRight));
            SaveResult(
                memory,
                124,
                (long)(shortLeft + longRight), // 124
                (long)(shortLeft - longRight), // 126
                (long)(shortLeft * longRight), // 128
                (long)(shortLeft / longRight), // 130
                (long)(shortLeft % longRight), // 132
                (long)(shortLeft & longRight), // 134
                (long)(shortLeft | longRight), // 136
                (long)(shortLeft ^ longRight)); // 138

            var ulongRight = (ulong)input;
            SaveResult(memory, 140, (long)(shortLeft << (int)ulongRight));
            SaveResult(memory, 142, (long)(shortLeft >> (int)ulongRight));
        }

        public virtual void UshortBinaryOperatorExpressionVariations(SimpleMemory memory)
        {
            var input = memory.ReadInt32(0);
            var ushortLeft = (ushort)input;

            var byteRight = (byte)input;
            SaveResult(memory, 0, (long)(ushortLeft << byteRight));
            SaveResult(memory, 2, (long)(ushortLeft >> byteRight));
            SaveResult(
                memory,
                4,
                (long)(ushortLeft + byteRight), // 4
                (long)(ushortLeft - byteRight), // 6
                (long)(ushortLeft * byteRight), // 8
                (long)(ushortLeft / byteRight), // 10
                (long)(ushortLeft % byteRight), // 12
                (long)(ushortLeft & byteRight), // 14
                (long)(ushortLeft | byteRight), // 16
                (long)(ushortLeft ^ byteRight)); // 18

            var sbyteRight = (sbyte)input;
            SaveResult(memory, 20, (long)(ushortLeft << sbyteRight));
            SaveResult(memory, 22, (long)(ushortLeft >> sbyteRight));
            SaveResult(
                memory,
                24,
                (long)(ushortLeft + sbyteRight), // 24
                (long)(ushortLeft - sbyteRight), // 26
                (long)(ushortLeft * sbyteRight), // 28
                (long)(ushortLeft / sbyteRight), // 30
                (long)(ushortLeft % sbyteRight), // 32
                (long)(ushortLeft & sbyteRight), // 34
                (long)(ushortLeft | sbyteRight), // 36
                (long)(ushortLeft ^ sbyteRight)); // 38

            var shortRight = (short)input;
            SaveResult(memory, 40, (long)(ushortLeft << shortRight));
            SaveResult(memory, 42, (long)(ushortLeft >> shortRight));
            SaveResult(
                memory,
                44,
                (long)(ushortLeft + shortRight), // 44
                (long)(ushortLeft - shortRight), // 46
                (long)(ushortLeft * shortRight), // 48
                (long)(ushortLeft / shortRight), // 50
                (long)(ushortLeft % shortRight), // 52
                (long)(ushortLeft & shortRight), // 54
                (long)(ushortLeft | shortRight), // 56
                (long)(ushortLeft ^ shortRight)); // 58

            var ushortRight = (ushort)input;
            SaveResult(memory, 60, (long)(ushortLeft << ushortRight));
            SaveResult(memory, 62, (long)(ushortLeft >> ushortRight));
            SaveResult(
                memory,
                64,
                (long)(ushortLeft + ushortRight), // 64
                (long)(ushortLeft - ushortRight), // 66
                (long)(ushortLeft * ushortRight), // 68
                (long)(ushortLeft / ushortRight), // 70
                (long)(ushortLeft % ushortRight), // 72
                (long)(ushortLeft & ushortRight), // 74
                (long)(ushortLeft | ushortRight), // 76
                (long)(ushortLeft ^ ushortRight)); // 78

            var intRight = input;
            SaveResult(memory, 80, (long)(ushortLeft << intRight));
            SaveResult(memory, 82, (long)(ushortLeft >> intRight));
            SaveResult(
                memory,
                84,
                (long)(ushortLeft + intRight), // 84
                (long)(ushortLeft - intRight), // 86
                (long)(ushortLeft * intRight), // 88
                (long)(ushortLeft / intRight), // 90
                (long)(ushortLeft % intRight), // 92
                (long)(ushortLeft & intRight), // 94
                (long)(ushortLeft | intRight), // 96
                (long)(ushortLeft ^ intRight)); // 98

            var uintRight = (uint)input;
            SaveResult(memory, 100, (long)(ushortLeft << (int)uintRight));
            SaveResult(memory, 102, (long)(ushortLeft >> (int)uintRight));
            SaveResult(
                memory,
                104,
                (long)(ushortLeft + uintRight), // 104
                (long)(ushortLeft - uintRight), // 106
                (long)(ushortLeft * uintRight), // 108
                (long)(ushortLeft / uintRight), // 110
                (long)(ushortLeft % uintRight), // 112
                (long)(ushortLeft & uintRight), // 114
                (long)(ushortLeft | uintRight), // 116
                (long)(ushortLeft ^ uintRight)); // 118

            var longRight = (long)input;
            SaveResult(memory, 120, (long)(ushortLeft << (int)longRight));
            SaveResult(memory, 122, (long)(ushortLeft >> (int)longRight));
            SaveResult(
                memory,
                124,
                (long)(ushortLeft + longRight), // 124
                (long)(ushortLeft - longRight), // 126
                (long)(ushortLeft * longRight), // 128
                (long)(ushortLeft / longRight), // 130
                (long)(ushortLeft % longRight), // 132
                (long)(ushortLeft & longRight), // 134
                (long)(ushortLeft | longRight), // 136
                (long)(ushortLeft ^ longRight)); // 138

            var ulongRight = (ulong)input;
            SaveResult(memory, 140, (long)(ushortLeft << (int)ulongRight));
            SaveResult(memory, 142, (long)(ushortLeft >> (int)ulongRight));
            SaveResult(
                memory,
                144,
                (long)(ushortLeft + ulongRight), // 144
                (long)(ushortLeft - ulongRight), // 146
                (long)(ushortLeft * ulongRight), // 148
                (long)(ushortLeft / ulongRight), // 150
                (long)(ushortLeft % ulongRight), // 152
                (long)(ushortLeft & ulongRight), // 154
                (long)(ushortLeft | ulongRight), // 156
                (long)(ushortLeft ^ ulongRight)); // 158
        }

        public virtual void IntBinaryOperatorExpressionVariations(SimpleMemory memory)
        {
            var input = memory.ReadInt32(0);
            var intLeft = input;

            var byteRight = (byte)input;
            SaveResult(memory, 0, (long)(intLeft << byteRight));
            SaveResult(memory, 2, (long)(intLeft >> byteRight));
            SaveResult(
                memory,
                4,
                (long)(intLeft + byteRight), // 4
                (long)(intLeft - byteRight), // 6
                (long)(intLeft * byteRight), // 8
                (long)(intLeft / byteRight), // 10
                (long)(intLeft % byteRight), // 12
                (long)(intLeft & byteRight), // 14
                (long)(intLeft | byteRight), // 16
                (long)(intLeft ^ byteRight)); // 18

            var sbyteRight = (sbyte)input;
            SaveResult(memory, 20, (long)(intLeft << sbyteRight));
            SaveResult(memory, 22, (long)(intLeft >> sbyteRight));
            SaveResult(
                memory,
                24,
                (long)(intLeft + sbyteRight), // 24
                (long)(intLeft - sbyteRight), // 26
                (long)(intLeft * sbyteRight), // 28
                (long)(intLeft / sbyteRight), // 30
                (long)(intLeft % sbyteRight), // 32
                (long)(intLeft & sbyteRight), // 34
                (long)(intLeft | sbyteRight), // 36
                (long)(intLeft ^ sbyteRight)); // 38

            var shortRight = (short)input;
            SaveResult(memory, 40, (long)(intLeft << shortRight));
            SaveResult(memory, 42, (long)(intLeft >> shortRight));
            SaveResult(
                memory,
                44,
                (long)(intLeft + shortRight), // 44
                (long)(intLeft - shortRight), // 46
                (long)(intLeft * shortRight), // 48
                (long)(intLeft / shortRight), // 50
                (long)(intLeft % shortRight), // 52
                (long)(intLeft & shortRight), // 54
                (long)(intLeft | shortRight), // 56
                (long)(intLeft ^ shortRight)); // 58

            var ushortRight = (ushort)input;
            SaveResult(memory, 60, (long)(intLeft << ushortRight));
            SaveResult(memory, 62, (long)(intLeft >> ushortRight));
            SaveResult(
                memory,
                64,
                (long)(intLeft + ushortRight), // 64
                (long)(intLeft - ushortRight), // 66
                (long)(intLeft * ushortRight), // 68
                (long)(intLeft / ushortRight), // 70
                (long)(intLeft % ushortRight), // 72
                (long)(intLeft & ushortRight), // 74
                (long)(intLeft | ushortRight), // 76
                (long)(intLeft ^ ushortRight)); // 78

            var intRight = input;
            SaveResult(memory, 80, (long)(intLeft << intRight));
            SaveResult(memory, 82, (long)(intLeft >> intRight));
            SaveResult(
                memory,
                84,
                (long)(intLeft + intRight), // 84
                (long)(intLeft - intRight), // 86
                (long)(intLeft * intRight), // 88
                (long)(intLeft / intRight), // 90
                (long)(intLeft % intRight), // 92
                (long)(intLeft & intRight), // 94
                (long)(intLeft | intRight), // 96
                (long)(intLeft ^ intRight)); // 98

            var uintRight = (uint)input;
            SaveResult(memory, 100, (long)(intLeft << (int)uintRight));
            SaveResult(memory, 102, (long)(intLeft >> (int)uintRight));
            SaveResult(
                memory,
                104,
                (long)(intLeft + uintRight), // 104
                (long)(intLeft - uintRight), // 106
                (long)(intLeft * uintRight), // 108
                (long)(intLeft / uintRight), // 110
                (long)(intLeft % uintRight), // 112
                (long)(intLeft & uintRight), // 114
                (long)(intLeft | uintRight), // 116
                (long)(intLeft ^ uintRight)); // 118

            var longRight = (long)input;
            SaveResult(memory, 120, (long)(intLeft << (int)longRight));
            SaveResult(memory, 122, (long)(intLeft >> (int)longRight));
            SaveResult(
                memory,
                124,
                (long)(intLeft + longRight), // 124
                (long)(intLeft - longRight), // 126
                (long)(intLeft * longRight), // 128
                (long)(intLeft / longRight), // 130
                (long)(intLeft % longRight), // 132
                (long)(intLeft & longRight), // 134
                (long)(intLeft | longRight), // 136
                (long)(intLeft ^ longRight)); // 138

            var ulongRight = (ulong)input;
            SaveResult(memory, 140, (long)(intLeft << (int)ulongRight));
            SaveResult(memory, 142, (long)(intLeft >> (int)ulongRight));
        }

        public virtual void UintBinaryOperatorExpressionVariations(SimpleMemory memory)
        {
            var input = memory.ReadUInt32(0);
            var uintLeft = input;

            var byteRight = (byte)input;
            SaveResult(memory, 0, (long)(uintLeft << byteRight));
            SaveResult(memory, 2, (long)(uintLeft >> byteRight));
            SaveResult(
                memory,
                4,
                (long)(uintLeft + byteRight), // 4
                (long)(uintLeft - byteRight), // 6
                (long)(uintLeft * byteRight), // 8
                (long)(uintLeft / byteRight), // 10
                (long)(uintLeft % byteRight), // 12
                (long)(uintLeft & byteRight), // 14
                (long)(uintLeft | byteRight), // 16
                (long)(uintLeft ^ byteRight)); // 18

            var sbyteRight = (sbyte)input;
            SaveResult(memory, 20, (long)(uintLeft << sbyteRight));
            SaveResult(memory, 22, (long)(uintLeft >> sbyteRight));
            SaveResult(
                memory,
                24,
                (long)(uintLeft + sbyteRight), // 24
                (long)(uintLeft - sbyteRight), // 26
                (long)(uintLeft * sbyteRight), // 28
                (long)(uintLeft / sbyteRight), // 30
                (long)(uintLeft % sbyteRight), // 32
                (long)(uintLeft & sbyteRight), // 34
                (long)(uintLeft | sbyteRight), // 36
                (long)(uintLeft ^ sbyteRight)); // 38

            var shortRight = (short)input;
            SaveResult(memory, 40, (long)(uintLeft << shortRight));
            SaveResult(memory, 42, (long)(uintLeft >> shortRight));
            SaveResult(
                memory,
                44,
                (long)(uintLeft + shortRight), // 44
                (long)(uintLeft - shortRight), // 46
                (long)(uintLeft * shortRight), // 48
                (long)(uintLeft / shortRight), // 50
                (long)(uintLeft % shortRight), // 52
                (long)(uintLeft & shortRight), // 54
                (long)(uintLeft | shortRight), // 56
                (long)(uintLeft ^ shortRight)); // 58

            var ushortRight = (ushort)input;
            SaveResult(memory, 60, (long)(uintLeft << ushortRight));
            SaveResult(memory, 62, (long)(uintLeft >> ushortRight));
            SaveResult(
                memory,
                64,
                (long)(uintLeft + ushortRight), // 64
                (long)(uintLeft - ushortRight), // 66
                (long)(uintLeft * ushortRight), // 68
                (long)(uintLeft / ushortRight), // 70
                (long)(uintLeft % ushortRight), // 72
                (long)(uintLeft & ushortRight), // 74
                (long)(uintLeft | ushortRight), // 76
                (long)(uintLeft ^ ushortRight)); // 78

            var intRight = (int)input;
            SaveResult(memory, 80, (long)(uintLeft << intRight));
            SaveResult(memory, 82, (long)(uintLeft >> intRight));
            SaveResult(
                memory,
                84,
                (long)(uintLeft + intRight), // 84
                (long)(uintLeft - intRight), // 86
                (long)(uintLeft * intRight), // 88
                (long)(uintLeft / intRight), // 90
                (long)(uintLeft % intRight), // 92
                (long)(uintLeft & intRight), // 94
                (long)(uintLeft | intRight), // 96
                (long)(uintLeft ^ intRight)); // 98

            var uintRight = (uint)input;
            SaveResult(memory, 100, (long)(uintLeft << (int)uintRight));
            SaveResult(memory, 102, (long)(uintLeft >> (int)uintRight));
            SaveResult(
                memory,
                104,
                (long)(uintLeft + uintRight), // 104
                (long)(uintLeft - uintRight), // 106
                (long)(uintLeft * uintRight), // 108
                (long)(uintLeft / uintRight), // 110
                (long)(uintLeft % uintRight), // 112
                (long)(uintLeft & uintRight), // 114
                (long)(uintLeft | uintRight), // 116
                (long)(uintLeft ^ uintRight)); // 118

            var longRight = (long)input;
            SaveResult(memory, 120, (long)(uintLeft << (int)longRight));
            SaveResult(memory, 122, (long)(uintLeft >> (int)longRight));
            SaveResult(
                memory,
                124,
                (long)(uintLeft + longRight), // 124
                (long)(uintLeft - longRight), // 126
                (long)(uintLeft * longRight), // 128
                (long)(uintLeft / longRight), // 130
                (long)(uintLeft % longRight), // 132
                (long)(uintLeft & longRight), // 134
                (long)(uintLeft | longRight), // 136
                (long)(uintLeft ^ longRight)); // 138

            var ulongRight = (ulong)input;
            SaveResult(memory, 140, (long)(uintLeft << (int)ulongRight));
            SaveResult(memory, 142, (long)(uintLeft >> (int)ulongRight));
            SaveResult(
                memory,
                144,
                (long)(uintLeft + ulongRight), // 144
                (long)(uintLeft - ulongRight), // 146
                (long)(uintLeft * ulongRight), // 148
                (long)(uintLeft / ulongRight), // 150
                (long)(uintLeft % ulongRight), // 152
                (long)(uintLeft & ulongRight), // 154
                (long)(uintLeft | ulongRight), // 156
                (long)(uintLeft ^ ulongRight)); // 158
        }

        public virtual void LongBinaryOperatorExpressionVariationsLow(SimpleMemory memory)
        {
            long input = ((long)memory.ReadInt32(0) << 32) | (uint)memory.ReadInt32(1);
            var longLeft = input;

            var byteRight = (byte)input;
            SaveResult(memory, 0, (long)(longLeft << byteRight));
            SaveResult(memory, 2, (long)(longLeft >> byteRight));
            SaveResult(
                memory,
                4,
                (long)(longLeft + byteRight), // 4
                (long)(longLeft - byteRight), // 6
                (long)(longLeft * byteRight), // 8
                (long)(longLeft / byteRight), // 10
                (long)(longLeft % byteRight), // 12
                (long)(longLeft & byteRight), // 14
                (long)(longLeft | byteRight), // 16
                (long)(longLeft ^ byteRight)); // 18

            var sbyteRight = (sbyte)input;
            SaveResult(memory, 20, (long)(longLeft << sbyteRight));
            SaveResult(memory, 22, (long)(longLeft >> sbyteRight));
            SaveResult(
                memory,
                24,
                (long)(longLeft + sbyteRight), // 24
                (long)(longLeft - sbyteRight), // 26
                (long)(longLeft * sbyteRight), // 28
                (long)(longLeft / sbyteRight), // 30
                (long)(longLeft % sbyteRight), // 32
                (long)(longLeft & sbyteRight), // 34
                (long)(longLeft | sbyteRight), // 36
                (long)(longLeft ^ sbyteRight)); // 38

            var shortRight = (short)input;
            SaveResult(memory, 40, (long)(longLeft << shortRight));
            SaveResult(memory, 42, (long)(longLeft >> shortRight));
            SaveResult(
                memory,
                44,
                (long)(longLeft + shortRight), // 44
                (long)(longLeft - shortRight), // 46
                (long)(longLeft * shortRight), // 48
                (long)(longLeft / shortRight), // 50
                (long)(longLeft % shortRight), // 52
                (long)(longLeft & shortRight), // 54
                (long)(longLeft | shortRight), // 56
                (long)(longLeft ^ shortRight)); // 58

            var ushortRight = (ushort)input;
            SaveResult(memory, 60, (long)(longLeft << ushortRight));
            SaveResult(memory, 62, (long)(longLeft >> ushortRight));
            SaveResult(
                memory,
                64,
                (long)(longLeft + ushortRight), // 64
                (long)(longLeft - ushortRight), // 66
                (long)(longLeft * ushortRight), // 68
                (long)(longLeft / ushortRight), // 70
                (long)(longLeft % ushortRight), // 72
                (long)(longLeft & ushortRight), // 74
                (long)(longLeft | ushortRight), // 76
                (long)(longLeft ^ ushortRight)); // 78
        }

        public virtual void LongBinaryOperatorExpressionVariationsHigh(SimpleMemory memory)
        {
            long input = ((long)memory.ReadInt32(0) << 32) | (uint)memory.ReadInt32(1);
            var longLeft = input;

            var intRight = (int)input;
            SaveResult(memory, 80, (long)(longLeft << intRight));
            SaveResult(memory, 82, (long)(longLeft >> intRight));
            SaveResult(
                memory,
                84,
                (long)(longLeft + intRight), // 84
                (long)(longLeft - intRight), // 86
                (long)(longLeft * intRight), // 88
                (long)(longLeft / intRight), // 90
                (long)(longLeft % intRight), // 92
                (long)(longLeft & intRight), // 94
                (long)(longLeft | intRight), // 96
                (long)(longLeft ^ intRight)); // 98

            var uintRight = (uint)input;
            SaveResult(memory, 100, (long)(longLeft << (int)uintRight));
            SaveResult(memory, 102, (long)(longLeft >> (int)uintRight));
            SaveResult(
                memory,
                104,
                (long)(longLeft + uintRight), // 104
                (long)(longLeft - uintRight), // 106
                (long)(longLeft * uintRight), // 108
                (long)(longLeft / uintRight), // 110
                (long)(longLeft % uintRight), // 112
                (long)(longLeft & uintRight), // 114
                (long)(longLeft | uintRight), // 116
                (long)(longLeft ^ uintRight)); // 118

            var longRight = (long)input;
            SaveResult(memory, 120, (long)(longLeft << (int)longRight));
            SaveResult(memory, 122, (long)(longLeft >> (int)longRight));
            SaveResult(
                memory,
                124,
                (long)(longLeft + longRight), // 124
                (long)(longLeft - longRight), // 126
                (long)(longLeft * longRight), // 128
                (long)(longLeft / longRight), // 130
                (long)(longLeft % longRight), // 132
                (long)(longLeft & longRight), // 134
                (long)(longLeft | longRight), // 136
                (long)(longLeft ^ longRight)); // 138

            var ulongRight = (ulong)input;
            SaveResult(memory, 140, (long)(longLeft << (int)ulongRight));
            SaveResult(memory, 142, (long)(longLeft >> (int)ulongRight));
        }

        public virtual void UlongBinaryOperatorExpressionVariationsLow(SimpleMemory memory)
        {
            ulong input = ((ulong)memory.ReadInt32(0) << 32) | (uint)memory.ReadInt32(1);
            var ulongLeft = input;

            var byteRight = (byte)input;
            SaveResult(memory, 0, (long)(ulongLeft << byteRight));
            SaveResult(memory, 2, (long)(ulongLeft >> byteRight));
            SaveResult(
                memory,
                4,
                (long)(ulongLeft + byteRight), // 4
                (long)(ulongLeft - byteRight), // 6
                (long)(ulongLeft * byteRight), // 8
                (long)(ulongLeft / byteRight), // 10
                (long)(ulongLeft % byteRight), // 12
                (long)(ulongLeft & byteRight), // 14
                (long)(ulongLeft | byteRight), // 16
                (long)(ulongLeft ^ byteRight)); // 18

            var sbyteRight = (sbyte)input;
            SaveResult(memory, 20, (long)(ulongLeft << sbyteRight));
            SaveResult(memory, 22, (long)(ulongLeft >> sbyteRight));

            var shortRight = (short)input;
            SaveResult(memory, 24, (long)(ulongLeft << shortRight));
            SaveResult(memory, 26, (long)(ulongLeft >> shortRight));

            var ushortRight = (ushort)input;
            SaveResult(memory, 28, (long)(ulongLeft << ushortRight));
            SaveResult(memory, 30, (long)(ulongLeft >> ushortRight));
            SaveResult(
                memory,
                32,
                (long)(ulongLeft + ushortRight), // 32
                (long)(ulongLeft - ushortRight), // 34
                (long)(ulongLeft * ushortRight), // 36
                (long)(ulongLeft / ushortRight), // 38
                (long)(ulongLeft % ushortRight), // 40
                (long)(ulongLeft & ushortRight), // 42
                (long)(ulongLeft | ushortRight), // 44
                (long)(ulongLeft ^ ushortRight)); // 46
        }

        public virtual void UlongBinaryOperatorExpressionVariationsHigh(SimpleMemory memory)
        {
            ulong input = ((ulong)memory.ReadInt32(0) << 32) | (uint)memory.ReadInt32(1);
            var ulongLeft = input;

            var intRight = (int)input;
            SaveResult(memory, 48, (long)(ulongLeft << intRight));
            SaveResult(memory, 50, (long)(ulongLeft >> intRight));

            var uintRight = (uint)input;
            SaveResult(memory, 52, (long)(ulongLeft << (int)uintRight));
            SaveResult(memory, 54, (long)(ulongLeft >> (int)uintRight));
            SaveResult(
                memory,
                56,
                (long)(ulongLeft + uintRight), // 56
                (long)(ulongLeft - uintRight), // 58
                (long)(ulongLeft * uintRight), // 60
                (long)(ulongLeft / uintRight), // 62
                (long)(ulongLeft % uintRight), // 64
                (long)(ulongLeft & uintRight), // 66
                (long)(ulongLeft | uintRight), // 68
                (long)(ulongLeft ^ uintRight)); // 70

            var longRight = (long)input;
            SaveResult(memory, 72, (long)(ulongLeft << (int)longRight));
            SaveResult(memory, 74, (long)(ulongLeft >> (int)longRight));

            var ulongRight = (ulong)input;
            SaveResult(memory, 76, (long)(ulongLeft << (int)ulongRight));
            SaveResult(memory, 78, (long)(ulongLeft >> (int)ulongRight));
            SaveResult(
                memory,
                80,
                (long)(ulongLeft + ulongRight), // 80
                (long)(ulongLeft - ulongRight), // 82
                (long)(ulongLeft * ulongRight), // 84
                (long)(ulongLeft / ulongRight), // 86
                (long)(ulongLeft % ulongRight), // 88
                (long)(ulongLeft & ulongRight), // 90
                (long)(ulongLeft | ulongRight), // 92
                (long)(ulongLeft ^ ulongRight)); // 94
        }

#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand

        public virtual void AllUnaryOperatorExpressionVariations(SimpleMemory memory)
        {
            long input = ((long)memory.ReadInt32(0) << 32) | (uint)memory.ReadInt32(1);

            var byteOperand = (byte)input;
            SaveResult(memory, 0, ~byteOperand);
            SaveResult(memory, 2, +byteOperand);
            SaveResult(memory, 4, -byteOperand);

            var sbyteOperand = (sbyte)input;
            SaveResult(memory, 6, ~sbyteOperand);
            SaveResult(memory, 8, +sbyteOperand);
            SaveResult(memory, 10, -sbyteOperand);

            var shortOperand = (short)input;
            SaveResult(memory, 12, ~shortOperand);
            SaveResult(memory, 14, +shortOperand);
            SaveResult(memory, 16, -shortOperand);

            var ushortOperand = (ushort)input;
            SaveResult(memory, 18, ~ushortOperand);
            SaveResult(memory, 20, +ushortOperand);
            SaveResult(memory, 22, -ushortOperand);

            var intOperand = (int)input;
            SaveResult(memory, 24, ~intOperand);
            SaveResult(memory, 26, +intOperand);
            SaveResult(memory, 28, -intOperand);

            var uintOperand = (uint)input;
            SaveResult(memory, 30, ~uintOperand);
            SaveResult(memory, 32, +uintOperand);
            SaveResult(memory, 34, -uintOperand);

            var longOperand = input;
            SaveResult(memory, 36, ~longOperand);
            SaveResult(memory, 38, +longOperand);
            SaveResult(memory, 40, -longOperand);

            var ulongOperand = (ulong)input;
            SaveResult(memory, 42, (long)(~ulongOperand));
            SaveResult(memory, 44, (long)(+ulongOperand));
        }

        public void ByteBinaryOperatorExpressionVariations(int input)
        {
            var memory = CreateMemory(160);
            memory.WriteInt32(0, input);
            ByteBinaryOperatorExpressionVariations(memory);
        }

        public void SbyteBinaryOperatorExpressionVariations(int input)
        {
            var memory = CreateMemory(144);
            memory.WriteInt32(0, input);
            SbyteBinaryOperatorExpressionVariations(memory);
        }

        public void ShortBinaryOperatorExpressionVariations(int input)
        {
            var memory = CreateMemory(144);
            memory.WriteInt32(0, input);
            ShortBinaryOperatorExpressionVariations(memory);
        }

        public void UshortBinaryOperatorExpressionVariations(int input)
        {
            var memory = CreateMemory(160);
            memory.WriteInt32(0, input);
            UshortBinaryOperatorExpressionVariations(memory);
        }

        public void IntBinaryOperatorExpressionVariations(int input)
        {
            var memory = CreateMemory(144);
            memory.WriteInt32(0, input);
            IntBinaryOperatorExpressionVariations(memory);
        }

        public void UintBinaryOperatorExpressionVariations(uint input)
        {
            var memory = CreateMemory(160);
            memory.WriteUInt32(0, input);
            UintBinaryOperatorExpressionVariations(memory);
        }

        public void LongBinaryOperatorExpressionVariationsLow(long input)
        {
            var memory = CreateMemory(144);
            memory.WriteInt32(0, (int)(input >> 32));
            memory.WriteInt32(1, (int)input);
            LongBinaryOperatorExpressionVariationsLow(memory);
        }

        public void LongBinaryOperatorExpressionVariationsHigh(long input)
        {
            var memory = CreateMemory(144);
            memory.WriteInt32(0, (int)(input >> 32));
            memory.WriteInt32(1, (int)input);
            LongBinaryOperatorExpressionVariationsHigh(memory);
        }

        public void UlongBinaryOperatorExpressionVariationsLow(ulong input)
        {
            var memory = CreateMemory(96);
            memory.WriteInt32(0, (int)(input >> 32));
            memory.WriteInt32(1, (int)input);
            UlongBinaryOperatorExpressionVariationsLow(memory);
        }

        public void UlongBinaryOperatorExpressionVariationsHigh(ulong input)
        {
            var memory = CreateMemory(96);
            memory.WriteInt32(0, (int)(input >> 32));
            memory.WriteInt32(1, (int)input);
            UlongBinaryOperatorExpressionVariationsHigh(memory);
        }

        public void AllUnaryOperatorExpressionVariations(long input)
        {
            var memory = CreateMemory(46);
            memory.WriteInt32(0, (int)(input >> 32));
            memory.WriteInt32(1, (int)input);
            AllUnaryOperatorExpressionVariations(memory);
        }


        private void SaveResult(
            SimpleMemory memory,
            int startCellIndex,
            long number0,
            long number1,
            long number2,
            long number3,
            long number4,
            long number5,
            long number6,
            long number7)
        {
            SaveResult(memory, startCellIndex, number0);
            SaveResult(memory, startCellIndex + 2, number1);
            SaveResult(memory, startCellIndex + 4, number2);
            SaveResult(memory, startCellIndex + 6, number3);
            SaveResult(memory, startCellIndex + 8, number4);
            SaveResult(memory, startCellIndex + 10, number5);
            SaveResult(memory, startCellIndex + 12, number6);
            SaveResult(memory, startCellIndex + 14, number7);
        }

        private void SaveResult(SimpleMemory memory, int startCellIndex, long number)
        {
            memory.WriteInt32(startCellIndex, (int)(number >> 32));
            memory.WriteInt32(startCellIndex + 1, (int)number);
        }
    }
}
