using Mono.Cecil;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class ObjectCreateExpressionExtensions
    {
        public static string GetConstructorFullName(this ObjectCreateExpression objectCreateExpression) =>
            objectCreateExpression.Annotation<MethodReference>()?.FullName;
    }
}
