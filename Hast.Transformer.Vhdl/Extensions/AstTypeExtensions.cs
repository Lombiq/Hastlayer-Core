using System.Linq;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AstTypeExtensions
    {
        public static AstType GetStoredTypeOfTaskResultArray(this AstType type)
        {
            return ((SimpleType)type).TypeArguments.Single();
        }
    }
}
