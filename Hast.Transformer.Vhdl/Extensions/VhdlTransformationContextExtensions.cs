using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;

namespace Hast.Transformer.Vhdl.Models
{
    public static class VhdlTransformationContextExtensions
    {
        public static bool UseSimpleMemory(this IVhdlTransformationContext transformationContext)
        {
            return transformationContext.GetTransformerConfiguration().UseSimpleMemory;
        }
    }
}
