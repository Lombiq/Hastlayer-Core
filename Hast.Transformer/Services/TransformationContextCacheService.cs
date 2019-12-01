using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp;

namespace Hast.Transformer.Services
{
    public class TransformationContextCacheService : ITransformationContextCacheService
    {
        private readonly MemoryCache _cache = MemoryCache.Default;


        public ITransformationContext GetTransformationContext(IEnumerable<string> assemblyPaths, string transformationId) =>
            _cache.Get(GetCacheKey(assemblyPaths, transformationId)) as ITransformationContext;

        public void SetTransformationContext(ITransformationContext transformationContext, IEnumerable<string> assemblyPaths)
        {
            _cache.Set(
                GetCacheKey(assemblyPaths, transformationContext.Id),
                transformationContext,
                new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromHours(5)
                });
        }


        private static string GetCacheKey(IEnumerable<string> assemblyPaths, string transformationId)
        {
            var fileHashes = string.Empty;

            foreach (var path in assemblyPaths)
            {
                using (var stream = File.OpenRead(path))
                using (var sha = new SHA256Managed())
                {
                    fileHashes += BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);;
                }
            }

            return "Hast.Transformer.TransformationContextCache." + fileHashes + " - " + transformationId.GetHashCode();
        }
    }
}
