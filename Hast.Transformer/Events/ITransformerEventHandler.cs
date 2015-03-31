using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard.Events;

namespace Hast.Transformer.Events
{
    public interface ITransformerEventHandler : IEventHandler
    {
        /// <summary>
        /// Fired when the syntax tree is built from the assemblies to transform.
        /// </summary>
        /// <param name="transformationContext">The full context of the transformation, including the syntax tree to transform.</param>
        void SyntaxTreeBuilt(ITransformationContext transformationContext);
    }
}
