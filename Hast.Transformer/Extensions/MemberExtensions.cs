using System.Linq;

namespace ICSharpCode.Decompiler.TypeSystem
{
    public static class MemberExtensions
    {
        public static string GetFullName(this IMember member)
        {
            var name = $"{member.ReturnType.FullName} {member.DeclaringType.FullName}::{member.Name}";
            if (member is IMethod method)
            {
                name += $"({string.Join(", ", method.Parameters.Select(parameter => parameter.Type.FullName))})";
            }
            return name;
        }
    }
}
