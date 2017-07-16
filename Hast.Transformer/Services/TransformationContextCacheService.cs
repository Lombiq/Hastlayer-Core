using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Services
{
    public class TransformationContextCacheService : ITransformationContextCacheService
    {
        private readonly MemoryCache _cache = MemoryCache.Default;


        public ITransformationContext GetTransformationContext(SyntaxTree unprocessedSyntaxTree, string transformationId) =>
            _cache.Get(GetCacheKey(unprocessedSyntaxTree, transformationId)) as ITransformationContext;

        public void SetTransformationContext(ITransformationContext transformationContext, SyntaxTree unprocessedSyntaxTree)
        {
            _cache.Set(
                GetCacheKey(unprocessedSyntaxTree, transformationContext.Id),
                transformationContext,
                new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromHours(5)
                });
        }


        private static string GetCacheKey(SyntaxTree unprocessedSyntaxTree, string transformationId) =>
             "Hast.Transformer.TransformationContextCache." +
            unprocessedSyntaxTree.ToString().GetHashCode().ToString() + " - " + transformationId.GetHashCode();
    }
}
