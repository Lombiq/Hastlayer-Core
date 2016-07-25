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
    /// Transformer for processing fields of compiler-generated DisplayClasses.
    /// </summary>
    public interface IDisplayClassFieldTransformer : IDependency
    {
        Task<IMemberTransformerResult> Transform(FieldDeclaration field, IVhdlTransformationContext context);
    }
}
