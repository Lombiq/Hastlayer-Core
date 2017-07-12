﻿using Hast.Transformer.Models;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class HardwareEntryPointMemberEntityDeclarationExtensions
    {
        /// <summary>
        /// Indicates whether the member is a hardware entry point, i.e. should be executable from the host computer.
        /// </summary>
        public static bool IsHardwareEntryPointMember(this EntityDeclaration member)
        {
            return member.GetHardwareEntryPointMemberMetadata() != null && member.GetHardwareEntryPointMemberMetadata().IsHardwareEntryPointMember;
        }

        internal static void SetHardwareEntryPointMember(this EntityDeclaration member)
        {
            var metadata = member.GetHardwareEntryPointMemberMetadata();

            if (metadata == null)
            {
                metadata = new HardwareEntryPointMemberMetadata();
                member.AddAnnotation(metadata);
            }

            metadata.IsHardwareEntryPointMember = true;
        }

        internal static HardwareEntryPointMemberMetadata GetHardwareEntryPointMemberMetadata(this EntityDeclaration member)
        {
            return member.Annotation<HardwareEntryPointMemberMetadata>();
        }
    }
}