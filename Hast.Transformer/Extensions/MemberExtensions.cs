using System.Linq;

namespace ICSharpCode.Decompiler.TypeSystem
{
    public static class MemberExtensions
    {
        public static string GetFullName(this IMember member)
        {
            var name = member.ReturnType.FullName + " ";

            var declaringTypeNames = string.Empty;
            var currentType = member.DeclaringType;
            while (currentType.DeclaringType != null)
            {
                declaringTypeNames = "/" + currentType.Name + declaringTypeNames;
                currentType = currentType.DeclaringType;
            }

            name += $"{currentType.FullName}{declaringTypeNames}::{member.Name}";

            if (member is IMethod method)
            {
                name += $"({string.Join(", ", method.Parameters.Select(parameter => parameter.Type.FullName))})";
            }

            return name;
        }
    }
}
