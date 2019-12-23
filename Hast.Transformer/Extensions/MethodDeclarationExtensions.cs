using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class MethodDeclarationExtensions
    {
        public static bool IsConstructor(this MethodDeclaration methodDeclaration) =>
            (methodDeclaration.GetResolveResult<MemberResolveResult>()?.Member as IMethod)?.IsConstructor == true;
    }
}
