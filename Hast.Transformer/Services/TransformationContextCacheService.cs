using Hast.Transformer.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Hast.Transformer.Services
{
    public class TransformationContextCacheService : ITransformationContextCacheService
    {
        private readonly IMemoryCache _cache;

        public TransformationContextCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public ITransformationContext GetTransformationContext(IEnumerable<string> assemblyPaths, string transformationId) =>
            _cache.Get(GetCacheKey(assemblyPaths, transformationId)) as ITransformationContext;

        public void SetTransformationContext(ITransformationContext transformationContext, IEnumerable<string> assemblyPaths) => _cache.Set(
                GetCacheKey(assemblyPaths, transformationContext.Id),
                transformationContext,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(5),
                });

        private static string GetCacheKey(IEnumerable<string> assemblyPaths, string transformationId)
        {
            var fileHashes = new StringBuilder();

            foreach (var path in assemblyPaths)
            {
                using var stream = File.OpenRead(path);
                using var sha = new SHA256Managed();
                fileHashes.Append(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty, StringComparison.Ordinal));
            }

            return "Hast.Transformer.TransformationContextCache." + fileHashes + " - " + transformationId.GetHashCode(StringComparison.InvariantCulture);
        }
    }
}
