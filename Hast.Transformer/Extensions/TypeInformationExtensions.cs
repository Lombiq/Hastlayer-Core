using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ICSharpCode.Decompiler.Ast
{
    public static class TypeInformationExtensions
    {
        public static TypeReference GetActualType(this TypeInformation typeInformation)
        {
            return typeInformation.InferredType ?? typeInformation.ExpectedType;
        }
    }
}
