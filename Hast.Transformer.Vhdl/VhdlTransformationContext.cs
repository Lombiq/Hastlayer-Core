using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl
{
    public class VhdlTransformationContext : TransformationContext, IVhdlTransformationContext
    {
        public Module Module { get; set; }
        public IList<InterfaceMethodDefinition> InterfaceMethods { get; set; }
        public MethodCallChainTable MethodCallChainTable { get; set; }


        public VhdlTransformationContext(ITransformationContext previousContext) : base(previousContext)
        {
            InterfaceMethods = new List<InterfaceMethodDefinition>();
        }
    }
}
