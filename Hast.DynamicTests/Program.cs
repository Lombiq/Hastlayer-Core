using Hast.DynamicTests.Tests;
using System.Threading.Tasks;

namespace Hast.DynamicTests
{
    class Program
    {
        public static Task Main(string[] args)
        {
            // Uncomment this to generate the content of BinaryAndUnaryOperatorExpressionCases. 
            //TestInputs.Dynamic.BinaryAndUnaryOperatorExpressionCasesGenerator.Generate();

            // Eventually if hardware generation and execution will be automated end-to-end then these tests can be
            // executed as usual. However, for now (and also for more comfortable local testing) you can execute a
            // test simply like this too.
            return new BinaryAndUnaryOperatorExpressionCasesTests().ByteBinaryOperatorExpressionVariations();
            //await new CastExpressionCasesTests().AllNumberCastingVariations();
            //await new InlinedCasesTests().InlinedMultiReturn();
        }
    }
}
