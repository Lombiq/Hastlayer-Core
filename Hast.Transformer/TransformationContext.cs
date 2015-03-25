using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer
{
    public class TransformationContext : ITransformationContext
    {
        public string Id { get; set; }
        public SyntaxTree SyntaxTree { get; set; }
        public IHardwareGenerationConfiguration HardwareGenerationConfiguration { get; set; }
        public Func<AstType, TypeDeclaration> LookupDeclarationDelegate { private get; set; }


        public TransformationContext(ITransformationContext previousContext) : this()
        {
            Id = previousContext.Id;
            SyntaxTree = previousContext.SyntaxTree;
            HardwareGenerationConfiguration = previousContext.HardwareGenerationConfiguration;
            LookupDeclarationDelegate = previousContext.LookupDeclaration;
        }

        public TransformationContext()
        {
        }


        public TypeDeclaration LookupDeclaration(AstType type)
        {
            return LookupDeclarationDelegate(type);
        }
    }
}
