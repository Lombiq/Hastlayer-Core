
namespace ICSharpCode.NRefactory.CSharp
{
    public static class SimpleMemoryParameterDeclarationExtensions
    {
        public static bool IsSimpleMemoryParameter(this ParameterDeclaration parameterDeclaration)
        {
            var type = parameterDeclaration.Type as SimpleType;
            if (type == null) return false;
            return type.Identifier == "SimpleMemory";
        }
    }
}
