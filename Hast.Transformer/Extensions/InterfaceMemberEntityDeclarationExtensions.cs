using Hast.Transformer.Models;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class InterfaceMemberEntityDeclarationExtensions
    {
        public static bool IsInterfaceMember(this EntityDeclaration member)
        {
            return member.GetInterfaceMemberMetadata() != null && member.GetInterfaceMemberMetadata().IsInterfaceMember;
        }

        internal static void SetInterfaceMember(this EntityDeclaration member)
        {
            var metadata = member.GetInterfaceMemberMetadata();

            if (metadata == null)
            {
                metadata = new InterfaceMemberMetadata();
                member.AddAnnotation(metadata);
            }

            metadata.IsInterfaceMember = true;
        }

        internal static InterfaceMemberMetadata GetInterfaceMemberMetadata(this EntityDeclaration member)
        {
            return member.Annotation<InterfaceMemberMetadata>();
        }
    }
}
