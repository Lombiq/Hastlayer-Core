using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IExpressionTransformer : IDependency
    {
        /// <summary>
        /// Transforms an expression into a VHDL element that can be used in place of the original expression. Be aware
        /// that <code>currentBlock</code>, being a reference, can change.
        /// </summary>
        IVhdlElement Transform(Expression expression, ISubTransformerContext context);
    }
}
