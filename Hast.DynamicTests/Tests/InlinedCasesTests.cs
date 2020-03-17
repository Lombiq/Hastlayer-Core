using Hast.TestInputs.Dynamic;
using System.Threading.Tasks;
using Xunit;

namespace Hast.DynamicTests.Tests
{
    public class InlinedCasesTests
    {
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
