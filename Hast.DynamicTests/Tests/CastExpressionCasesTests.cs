using Hast.TestInputs.Dynamic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.DynamicTests.Tests
{
    [TestFixture]
    public class CastExpressionCasesTests
    {
        [Test]
        public Task AllNumberCastingVariations() =>
            TestExecutor.ExecuteSelectedTest<CastExpressionCases>(
                c => c.AllNumberCastingVariations(null),
                c =>
                {
                    c.AllNumberCastingVariations(long.MinValue + 1);
                    c.AllNumberCastingVariations(123);
                    c.AllNumberCastingVariations(124);
                    c.AllNumberCastingVariations(long.MaxValue);
                });
    }
}
