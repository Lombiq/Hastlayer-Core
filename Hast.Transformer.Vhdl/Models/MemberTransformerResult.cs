using System.Collections.Generic;
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
