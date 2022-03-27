using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Models;

internal class DeclarationReferenceMetadata
{
    public HashSet<AstNode> ReferencedFrom { get; private set; }
    public int ReferenceCount => ReferencedFrom.Count;
    public bool IsReferenced => ReferenceCount > 0;
    public bool WasVisited { get; set; }

    public DeclarationReferenceMetadata() => ReferencedFrom = new HashSet<AstNode>();
}
