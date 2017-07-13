using Hast.Layer;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    public interface IInvocationInstanceCountAdjuster : IDependency
    {
        void AdjustInvocationInstanceCounts(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
