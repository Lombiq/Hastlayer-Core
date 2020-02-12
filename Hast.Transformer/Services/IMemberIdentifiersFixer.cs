using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// If a method is called from within its own class then the target of the <see cref="InvocationExpression"/> will
    /// be an <see cref="IdentifierExpression"/>. However, it should really be a 
    /// <see cref="MemberReferenceExpression"/> as it was in ILSpy prior to v3 and as it is if it's called from another
    /// class. See: <see href="https://github.com/icsharpcode/ILSpy/issues/1407"/>.
    /// </summary>
    public interface IMemberIdentifiersFixer : IDependency
    {
        void FixMemberIdentifiers(SyntaxTree syntaxTree);
    }
}
