using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// A composition of <see cref="PatternStatementTransform"/> with the sole purpose of not transforming simple while
    /// statements back to more complex for statements like it would do if executed directly. Other parts of that
    /// transform, like auto-property re-creation is useful.
    /// </summary>
    internal class NoForPatternStatementTransform : ContextTrackingVisitor<AstNode>, IAstTransform
    {
        private readonly PatternStatementTransform _patternStatementTransform;


        public NoForPatternStatementTransform(DecompilerContext context) : base(context)
        {
            _patternStatementTransform = new PatternStatementTransform(context);
        }


        public override AstNode VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration, object data)
        {
            return _patternStatementTransform.VisitPropertyDeclaration(propertyDeclaration, data);
        }
    }
}
