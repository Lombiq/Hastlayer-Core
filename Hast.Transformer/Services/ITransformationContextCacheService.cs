using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    public interface ITransformationContextCacheService : IDependency
    {
        ITransformationContext GetTransformationContext(SyntaxTree unprocessedSyntaxTree, string transformationId);
        void SetTransformationContext(ITransformationContext transformationContext, SyntaxTree unprocessedSyntaxTree);
    }
}
