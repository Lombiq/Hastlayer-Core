using Hast.Common.Interfaces;
using Hast.Layer;
using Hast.Synthesis.Abstractions;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Replaces <see langword="static"/> <see langword="readonly"/> fields with literal value substitution. Only
    /// applies to fields that have the <see cref="ReplaceableAttribute"/>.
    /// </summary>
    /// <example>
    /// <para>If you have this code:</para>
    /// <code>
    ///     public class ParallelAlgorithm
    ///     {
    ///         [Replaceable(nameof(ParallelAlgorithm) + "." + nameof(MaxDegreeOfParallelism))]
    ///         private static readonly int MaxDegreeOfParallelism = 260;
    ///         // ..
    ///         public virtual void Run(SimpleMemory memory)
    ///         {
    ///             var input = memory.ReadInt32(Run_InputInt32Index);
    ///             var tasks = new Task&lt;int&gt;[MaxDegreeOfParallelism];
    /// </code>
    /// <para>The last line becomes by default:</para>
    /// <code>
    ///             var tasks = new Task&lt;int&gt;[260];
    /// </code>
    /// <para>But with substitution of 123:</para>
    /// <code>
    ///             var tasks = new Task&lt;int&gt;[123];
    /// </code>
    /// </example>
    public interface IReadonlyToConstConverter : IDependency
    {
        /// <summary>
        /// Finds and replaces any <see langword="public"/> <see langword="static"/> <see langword="readonly"/> fields
        /// in <paramref name="syntaxTree"/> that have <see cref="ReplaceableAttribute"/> with their assigned literal
        /// value. If <see cref="IHardwareGenerationConfiguration.CustomConfiguration"/> in
        /// <paramref name="configuration"/> has a <c>ReplaceableDynamicConstants</c> of type <c>IDictionary&lt;string, string&gt;</c>
        /// with the matching value identified by the attribute, then it is used instead of the assigned value. Read
        /// "Using dynamic constants" in <c>Docs/DevelopingHastlayer.md</c> for usage information.
        /// </summary>
        /// <param name="syntaxTree">The code to transform.</param>
        /// <param name="configuration">The holder of the custom configuration.</param>
        void ConvertReadonlyPrimitives(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
