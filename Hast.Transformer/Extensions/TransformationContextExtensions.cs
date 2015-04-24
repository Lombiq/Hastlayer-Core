using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;

namespace Hast.Transformer.Models
{
    public static class TransformationContextExtensions
    {
        public static TransformerConfiguration GetTransformerConfiguration(this ITransformationContext transformationContext)
        {
            return transformationContext.HardwareGenerationConfiguration.GetTransformerConfiguration();
        }
    }
}
