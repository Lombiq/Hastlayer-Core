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
    public interface IMethodTransformer : IDependency
    {
        Task<IMemberTransformerResult> Transform(MethodDeclaration method, IVhdlTransformationContext context);
    }
}
