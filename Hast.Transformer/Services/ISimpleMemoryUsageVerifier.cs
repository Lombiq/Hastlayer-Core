using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

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
