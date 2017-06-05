using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Produces data types that can be used in variable, signal, etc. declarations.
    /// </summary>
    public interface IDeclarableTypeCreator : IDependency
    {
        DataType CreateDeclarableType(AstNode valueHolder, AstType type, IVhdlTransformationContext context);
    }
}
