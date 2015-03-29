using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Models
{
    internal class DeclarationReferenceMetadata
    {
        public HashSet<AstNode> ReferencedFrom { get; private set; }
        public int ReferenceCount { get { return ReferencedFrom.Count; } }
        public bool IsReferenced { get { return ReferenceCount > 0; } }


        public DeclarationReferenceMetadata()
        {
            ReferencedFrom = new HashSet<AstNode>();
        }
    }
}
