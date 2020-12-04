using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    public interface IGeneratedTaskArraysInliner : IDependency
    {
        /// <summary>
        /// Get rid of unnecessary compiler-generated <see cref="System.Threading.Tasks.Task"/> arrays.
        /// </summary>
        /// <example>
        /// <para>
        /// If <see cref="System.Threading.Tasks.Task"/> objects are saved to an array sometimes the compiler generates
        /// another array variable without any apparent use. This makes transforming needlessly more complicated. E.g.
        /// </para>
        ///
        /// <code>
        /// Task&lt;bool&gt;[] array;
        /// Task&lt;bool&gt;[] arg_95_0;
        /// array = new Task&lt;bool&gt;[35];
        /// while (j &lt; 35)
        /// {
        ///     arg_95_0 = array;
        ///     arg_95_0[arg_95_1] = arg_90_0.StartNew&lt;bool&gt;(arg_90_1, j);
        ///     j = j + 1;
        /// }
        /// Task.WhenAll&lt;bool&gt;(array).Wait();
        /// </code>
        ///
        /// <para>
        /// Note that while there was the variable named <c>array</c> the compiler created <c>arg_95_0</c> and used it
        /// instead, but just inside the loop.
        /// </para>
        /// </example>
        void InlineGeneratedTaskArrays(SyntaxTree syntaxTree);
    }
}
