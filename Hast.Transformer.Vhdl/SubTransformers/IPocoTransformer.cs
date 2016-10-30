using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Transformer for processing POCOs (Plain Old C# Object) to handle e.g. properties.
    /// </summary>
    public interface IPocoTransformer : IDependency
    {
        bool IsSupportedMember(AstNode node);
        Task<IMemberTransformerResult> Transform(TypeDeclaration typeDeclaration, IVhdlTransformationContext context);
    }
}
