using System.Linq;

namespace ICSharpCode.Decompiler.CSharp
{
    public static class AstTypeExtensions
    {
        public static AstType GetStoredTypeOfTaskResultArray(this AstType type)
        {
            return ((SimpleType)type).TypeArguments.Single();
        }
    }
}
