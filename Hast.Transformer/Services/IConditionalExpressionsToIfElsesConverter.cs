using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts a conditional expression, i.e. an expression with a ternary operator into an if-else statement.
    /// </summary>
    /// <example>
    /// The following expression:
    /// numberOfStepsInIteration = testMode ? 1 : KpzKernels.GridWidth * KpzKernels.GridHeight;
    ///
    /// ...will be converted into the below form:
    /// if (testMode) numberOfStepsInIteration = 1;
    /// else numberOfStepsInIteration = KpzKernels.GridWidth * KpzKernels.GridHeight;.
    /// </example>
    public interface IConditionalExpressionsToIfElsesConverter : IDependency
    {
        void ConvertConditionalExpressionsToIfElses(SyntaxTree syntaxTree);
    }
}
