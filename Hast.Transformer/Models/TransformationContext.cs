using Hast.Common.Configuration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Models
{
    public class TransformationContext : ITransformationContext
    {
        public string Id { get; set; }
        public SyntaxTree SyntaxTree { get; set; }
        public IHardwareGenerationConfiguration HardwareGenerationConfiguration { get; set; }
        public ITypeDeclarationLookupTable TypeDeclarationLookupTable { get; set; }


        public TransformationContext(ITransformationContext previousContext) : this()
        {
            Id = previousContext.Id;
            SyntaxTree = previousContext.SyntaxTree;
            HardwareGenerationConfiguration = previousContext.HardwareGenerationConfiguration;
            TypeDeclarationLookupTable = previousContext.TypeDeclarationLookupTable;
        }

        public TransformationContext()
        {
        }
    }
}
