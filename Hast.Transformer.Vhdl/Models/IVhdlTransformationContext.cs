using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public interface IVhdlTransformationContext : ITransformationContext
    {
        Module Module { get; }
        IList<InterfaceMethodDefinition> InterfaceMethods { get; }
        MethodCallChainTable MethodCallChainTable { get; }
    }
}
