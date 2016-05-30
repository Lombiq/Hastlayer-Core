using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Visitors;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    public interface IGeneratedTaskArraysInliner : IDependency
    {
        /// <summary>
        /// Get rid of compiler-generated <see cref="System.Threading.Tasks.Task"/> arrays.
        /// </summary>
        /// <example>
        /// If <see cref="System.Threading.Tasks.Task"/> objects are saved to an array sometimes the compiler generates
        /// another array variable without any apparent use. This makes transforming needlessly more complicated. E.g.:
        /// 
        /// <c>
        /// Task<bool>[] array;
        /// Task<bool>[] arg_95_0;
        /// array = new Task<bool>[35];
        /// while (j < 35)
        /// {
        ///     arg_95_0 = array;
        ///     arg_95_0[arg_95_1] = arg_90_0.StartNew<bool>(arg_90_1, j);
        ///     j = j + 1;
        /// }
        /// Task.WhenAll<bool> (array).Wait();
        /// </c>
        /// 
        /// Note that while there was the variable named <c>array</c> the compiler created <c>arg_95_0</c> and used it
        /// instead, but just inside the loop.
        /// </example>
        /// <param name="syntaxTree"></param>
        void InlineGeneratedTaskArrays(SyntaxTree syntaxTree);
    }


    public class GeneratedTaskArraysInliner : IGeneratedTaskArraysInliner
    {
        public void InlineGeneratedTaskArrays(SyntaxTree syntaxTree)
        {
            var inlinableTaskArraysFindingVisitor = new InlinableTaskArraysFindingVisitor();
            syntaxTree.AcceptVisitor(inlinableTaskArraysFindingVisitor);
            syntaxTree.AcceptVisitor(new InlinableTaskArraysInliningVisitor(inlinableTaskArraysFindingVisitor.InlinableVariableMapping));
        }
    }
}
