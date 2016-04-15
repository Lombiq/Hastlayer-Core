using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.Models
{
    internal class MemberTransformerResult : IMemberTransformerResult
    {
        public EntityDeclaration Member { get; set; }
        public bool IsInterfaceMember { get; set; }
        public IEnumerable<IArchitectureComponentResult> ArchitectureComponentResults { get; set; }
    }
}
