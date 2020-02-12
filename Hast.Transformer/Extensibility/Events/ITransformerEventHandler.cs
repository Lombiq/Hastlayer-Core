using Hast.Common.Interfaces;
using Hast.Transformer.Models;

namespace Hast.Transformer.Extensibility.Events
{
    public interface ITransformerEventHandler : IEventHandler
    {
        /// <summary>
        /// Fired when the syntax tree is built from the assemblies to transform.
        /// </summary>
        /// <param name="transformationContext">
        /// The full context of the transformation, including the syntax tree to transform.
        /// </param>
        void SyntaxTreeBuilt(ITransformationContext transformationContext);
    }
}
