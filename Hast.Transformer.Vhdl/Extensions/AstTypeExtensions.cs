using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AstTypeExtensions
    {
        public static bool IsTask(this AstType type)
        {
            return type is SimpleType && ((SimpleType)type).Identifier == "Task";
        }

        public static AstType GetStoredTypeOfTaskResultArray(this AstType type)
        {
            return ((SimpleType)type).TypeArguments.Single();
        }
    }
}
