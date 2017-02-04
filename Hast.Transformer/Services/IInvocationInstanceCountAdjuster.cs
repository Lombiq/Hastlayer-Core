using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    public interface IInvocationInstanceCountAdjuster : IDependency
    {
        void AdjustInvocationInstanceCounts(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
