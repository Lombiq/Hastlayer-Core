using Hast.Common.Configuration;
using Hast.Transformer.Abstractions.Configuration;

namespace Hast.Transformer.Models
{
    public static class TransformationContextExtensions
    {
        public static TransformerConfiguration GetTransformerConfiguration(this ITransformationContext transformationContext)
        {
            return transformationContext.HardwareGenerationConfiguration.TransformerConfiguration();
        }
    }
}
