using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Service for verifying the syntax tree so every usage of SimpleMemory is OK.
    /// </summary>
    public interface ISimpleMemoryUsageVerifier : IDependency
    {
        void VerifySimpleMemoryUsage(SyntaxTree syntaxTree);
    }
}
