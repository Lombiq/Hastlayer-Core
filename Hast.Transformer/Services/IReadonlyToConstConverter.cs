using Hast.Common.Interfaces;
using Hast.Layer;
using Hast.Synthesis.Abstractions;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Replaces <see langword="public"/> <see langword="static"/> <see langword="readonly"/> fields with literal value
    /// substitution. Only applies to fields that have the <see cref="ReplaceableAttribute"/>.
    /// </summary>
    /// <example>
    /// </example>
    public interface IReadonlyToConstConverter : IDependency
    {
        /// <summary>
        /// Finds and replaces any <see langword="public"/> <see langword="static"/> <see langword="readonly"/> fields
        /// in <paramref name="syntaxTree"/> that have <see cref="ReplaceableAttribute"/> with their assigned literal
        /// value. If <see cref="IHardwareGenerationConfiguration.CustomConfiguration"/> in
        /// <paramref name="configuration"/> has a <c>Replaceable</c> of type <c>IDictionary&gt;string, string&lt;</c>
        /// with the matching value identified by the attribute, then it is used instead of the assigned value. Read
        /// "Using dynamic constants" in <c>Docs/DevelopingHastlayer.md</c> for usage information.
        /// </summary>
        /// <param name="syntaxTree">The code to transform.</param>
        /// <param name="configuration">The holder of the custom configuration.</param>
        void ConvertReadonlyPrimitives(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
