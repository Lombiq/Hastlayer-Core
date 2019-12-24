using ICSharpCode.Decompiler.TypeSystem;
using System.Linq;

namespace ICSharpCode.Decompiler.Semantics
{
    public static class MemberResolveResultExtensions
    {
        public static string GetFullName(this MemberResolveResult memberResolveResult)
        {
            var member = memberResolveResult.Member;
            var name = $"{member.ReturnType.FullName} {member.DeclaringType.FullName}::{member.Name}";
            if (member is IMethod method)
            {
                name += $"({string.Join(", ", method.Parameters.Select(parameter => parameter.Type.FullName))})";
            }
            return name;
        }
    }
}
