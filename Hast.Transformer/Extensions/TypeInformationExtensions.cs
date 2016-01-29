using Mono.Cecil;

namespace ICSharpCode.Decompiler.Ast
{
    public static class TypeInformationExtensions
    {
        public static TypeReference GetActualType(this TypeInformation typeInformation)
        {
            return typeInformation.ExpectedType ?? typeInformation.InferredType;
        }
    }
}
