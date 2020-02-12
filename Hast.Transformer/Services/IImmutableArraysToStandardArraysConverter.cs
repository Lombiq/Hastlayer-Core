using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts usages of <see cref="System.Collections.Immutable.ImmutableArray"/> to standard arrays. This is to make
    /// transformation easier, because the compile-time .NET checks being already done there is no need to handle such
    /// arrays differently.
    /// </summary>
    public interface IImmutableArraysToStandardArraysConverter : IDependency
    {
        void ConvertImmutableArraysToStandardArrays(SyntaxTree syntaxTree, IKnownTypeLookupTable knownTypeLookupTable);
    }
}
