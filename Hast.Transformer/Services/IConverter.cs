using Hast.Common.Interfaces;
using Hast.Layer;
using Hast.Transformer.Abstractions;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// A common interface for any converters utilized by <see cref="ITransformer"/>.
    /// </summary>
    public interface IConverter : IDependency
    {
        /// <summary>
        /// Performs a conversion operation by altering the <paramref name="syntaxTree"/>.
        /// </summary>
        void Convert(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
