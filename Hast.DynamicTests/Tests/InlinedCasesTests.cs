using Hast.TestInputs.Dynamic;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Hast.DynamicTests.Tests
{
    [TestFixture]
    public class InlinedCasesTests
    {
        [Test]
        public Task InlinedMultiReturn() =>
            TestExecutor.ExecuteSelectedTest<InlinedCases>(
                g => g.InlinedMultiReturn(null),
                g =>
                {
                    g.InlinedMultiReturn(3);
                    g.InlinedMultiReturn(-3);
                });
    }
}
