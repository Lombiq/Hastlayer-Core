using Hast.Common.Configuration;
using Hast.Transformer.Visitors;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    public interface IInvocationInstanceCountAdjuster : IDependency
    {
        void AdjustInvocationInstanceCounts(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }


    public class InvocationInstanceCountAdjuster : IInvocationInstanceCountAdjuster
    {
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;


        public InvocationInstanceCountAdjuster(ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory)
        {
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
        }


        public void AdjustInvocationInstanceCounts(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration)
        {
            syntaxTree.AcceptVisitor(new InvocationInstanceCountAdjustingVisitor(
                _typeDeclarationLookupTableFactory.Create(syntaxTree), 
                configuration));
        }
    }
}
