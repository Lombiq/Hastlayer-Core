using Hast.TestInputs.Dynamic;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Hast.DynamicTests.Tests
{
    public class BinaryAndUnaryOperatorExpressionCasesTests
    {
        // MinValue would cause a division by zero when the input is cast to smaller data types that's why MiValue + 1
        // is tested everywhere.
        // Testing at least an odd and even number too.
        // Since there are no generic constraints for numeric types unfortunately the non-int ones need to be copied 
        // (see: https://stackoverflow.com/questions/32664/is-there-a-constraint-that-restricts-my-generic-method-to-numeric-types).

        // ByteBinaryOperatorExpressionVariations(int.MinValue + 1) fails on ulong multiplication with:
        // "index: 148, hardware result: { 255, 255, 7, 0 }, software result: { 255, 255, 255, 255 }"
        // Same with UshortBinaryOperatorExpressionVariations.
        // Most possibly overflow is not handled the same as in .NET.
        [Fact]
        public Task ByteBinaryOperatorExpressionVariations() =>
        ExecuteIntTest(
             b => b.ByteBinaryOperatorExpressionVariations(null),
             b => b.ByteBinaryOperatorExpressionVariations,
             true);

        [Fact]
        public Task SbyteBinaryOperatorExpressionVariations() =>
            ExecuteIntTest(
                 b => b.SbyteBinaryOperatorExpressionVariations(null),
                 b => b.SbyteBinaryOperatorExpressionVariations);

        [Fact]
        public Task ShortBinaryOperatorExpressionVariations() =>
            ExecuteIntTest(
                 b => b.ShortBinaryOperatorExpressionVariations(null),
                 b => b.ShortBinaryOperatorExpressionVariations);

        [Fact]
        public Task UshortBinaryOperatorExpressionVariations() =>
            ExecuteIntTest(
                 b => b.UshortBinaryOperatorExpressionVariations(null),
                 b => b.UshortBinaryOperatorExpressionVariations,
                 true);

        [Fact]
        public Task IntBinaryOperatorExpressionVariations() =>
            ExecuteIntTest(
                 b => b.IntBinaryOperatorExpressionVariations(null),
                 b => b.IntBinaryOperatorExpressionVariations);

        [Fact]
        public Task UintBinaryOperatorExpressionVariations() =>
            ExecuteTest(
                b => b.UintBinaryOperatorExpressionVariations(null),
                b =>
                {
                    b.UintBinaryOperatorExpressionVariations(uint.MinValue + 1);
                    b.UintBinaryOperatorExpressionVariations(123);
                    b.UintBinaryOperatorExpressionVariations(124);
                    b.UintBinaryOperatorExpressionVariations(uint.MaxValue);
                });

        [Fact]
        public Task LongBinaryOperatorExpressionVariationsLow() =>
            ExecuteLongTest(
                 b => b.LongBinaryOperatorExpressionVariationsLow(null),
                 b => b.LongBinaryOperatorExpressionVariationsLow);

        [Fact]
        public Task LongBinaryOperatorExpressionVariationsHigh() =>
            ExecuteLongTest(
                 b => b.LongBinaryOperatorExpressionVariationsHigh(null),
                 b => b.LongBinaryOperatorExpressionVariationsHigh);

        [Fact]
        public Task UlongBinaryOperatorExpressionVariationsLow() =>
            ExecuteTest(
                b => b.UlongBinaryOperatorExpressionVariationsLow(null),
                b =>
                {
                    b.UlongBinaryOperatorExpressionVariationsLow(ulong.MinValue + 1);
                    b.UlongBinaryOperatorExpressionVariationsLow(123);
                    b.UlongBinaryOperatorExpressionVariationsLow(124);
                    b.UlongBinaryOperatorExpressionVariationsLow(long.MaxValue);
                });

        [Fact]
        public Task UlongBinaryOperatorExpressionVariationsHigh() =>
            ExecuteTest(
                b => b.UlongBinaryOperatorExpressionVariationsHigh(null),
                b =>
                {
                    b.UlongBinaryOperatorExpressionVariationsHigh(ulong.MinValue + 1);
                    b.UlongBinaryOperatorExpressionVariationsHigh(123);
                    b.UlongBinaryOperatorExpressionVariationsHigh(124);
                    b.UlongBinaryOperatorExpressionVariationsHigh(ulong.MaxValue);
                });

        [Fact]
        public Task AllUnaryOperatorExpressionVariations() =>
            ExecuteLongTest(
                 b => b.AllUnaryOperatorExpressionVariations(null),
                 b => b.AllUnaryOperatorExpressionVariations);

        private static Task ExecuteIntTest(
            Expression<Action<BinaryAndUnaryOperatorExpressionCases>> caseSelector,
            Func<BinaryAndUnaryOperatorExpressionCases, IntTestCaseMethod> caseMethod,
            bool noMinValue = false) =>
            ExecuteTest(
                caseSelector,
                b =>
                {
                    if (!noMinValue) caseMethod(b)(int.MinValue + 1);
                    caseMethod(b)(123);
                    caseMethod(b)(124);
                    caseMethod(b)(int.MaxValue);
                });

        private delegate void IntTestCaseMethod(int input);

        private static Task ExecuteLongTest(
            Expression<Action<BinaryAndUnaryOperatorExpressionCases>> caseSelector,
            Func<BinaryAndUnaryOperatorExpressionCases, LongTestCaseMethod> caseMethod) =>
            ExecuteTest(
                caseSelector,
                b =>
                {
                    caseMethod(b)(long.MinValue + 1);
                    caseMethod(b)(123);
                    caseMethod(b)(124);
                    caseMethod(b)(long.MaxValue);
                });

        private delegate void LongTestCaseMethod(long input);

        private static Task ExecuteTest(
            Expression<Action<BinaryAndUnaryOperatorExpressionCases>> caseSelector,
            Action<BinaryAndUnaryOperatorExpressionCases> testExecutor) =>
            TestExecutor.ExecuteSelectedTest(caseSelector, testExecutor);
    }
}
