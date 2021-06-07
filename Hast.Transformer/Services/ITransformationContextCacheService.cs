using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.Transformer.Models;

namespace Hast.Transformer.Services
{
    public interface ITransformationContextCacheService : IDependency
    {
        ITransformationContext GetTransformationContext(IEnumerable<string> assemblyPaths, string transformationId);
        void SetTransformationContext(ITransformationContext transformationContext, IEnumerable<string> assemblyPaths);
    }
}
