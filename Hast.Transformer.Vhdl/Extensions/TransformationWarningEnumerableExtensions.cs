using System.Collections.Generic;
using Hast.Common.Models;
using Hast.Layer;

namespace Hast.Transformer.Vhdl.Models
{
    public static class TransformationWarningEnumerableExtensions
    {
        public static void AddWarning(this IList<ITransformationWarning> warnings, string code, string message) =>
            warnings.Add(new TransformationWarning { Code = code, Message = message });
    }
}
