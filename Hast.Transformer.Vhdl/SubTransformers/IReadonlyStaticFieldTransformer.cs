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
    /// Transformer for processing read-only static fields.
    /// </summary>
    public interface IReadonlyStaticFieldTransformer : IDependency
    {
        bool CanTransform(FieldDeclaration field);
        Task<IMemberTransformerResult> Transform(FieldDeclaration field, IVhdlTransformationContext context);
    }
}
