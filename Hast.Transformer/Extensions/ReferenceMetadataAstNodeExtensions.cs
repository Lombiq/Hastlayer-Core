using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    internal static class ReferenceMetadataAstNodeExtensions
    {
        public static bool IsReferenced(this AstNode node)
        {
            var metadata = node.GetReferenceMetadata();
            return metadata != null && metadata.IsReferenced;
        }

        public static void AddReference(this AstNode node, AstNode from)
        {
            node.GetOrAddReferenceMetadata().ReferencedFrom.Add(from);
        }

        public static bool HasReferenceMetadata(this AstNode node)
        {
            return node.GetReferenceMetadata() != null;
        }

        public static DeclarationReferenceMetadata GetReferenceMetadata(this AstNode node)
        {
            return node.Annotation<DeclarationReferenceMetadata>();
        }

        public static DeclarationReferenceMetadata GetOrAddReferenceMetadata(this AstNode node)
        {
            var metadata = node.GetReferenceMetadata();
            if (metadata == null)
            {
                metadata = new DeclarationReferenceMetadata();
                node.AddAnnotation(metadata);
            }
            return metadata;
        }
    }
}
