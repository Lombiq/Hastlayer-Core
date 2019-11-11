using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace Hast.Transformer.Services
{
    public class DecompilationErrorsFixer : IDecompilationErrorsFixer
    {
        public void FixDecompilationErrors(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new DecompilationErrorsFixingVisitor());
        }


        private class DecompilationErrorsFixingVisitor : DepthFirstAstVisitor
        {
            public override void VisitCastExpression(CastExpression castExpression)
            {
                // Working around https://github.com/icsharpcode/ILSpy/issues/807 (until we update the ILSpy libraries).
                // Such buggy expressions always have the form "(ulong)negative value", like "(ulong)-131071". The
                // correct value can be retrieved by casting the value to uint (i.e. 4294836225 in this case).
                base.VisitCastExpression(castExpression);

                var primitiveExpression = castExpression.Expression as PrimitiveExpression;

                if (!castExpression.Type.Is<PrimitiveType>(primitive => primitive.KnownTypeCode == KnownTypeCode.UInt64) ||
                    (primitiveExpression == null || !(primitiveExpression.Value is int)))
                {
                    return;
                }

                var value = (int)primitiveExpression.Value;
                var correctValue = (uint)value;
                var clonedPrimitiveExpression = (PrimitiveExpression)primitiveExpression.Clone();
                clonedPrimitiveExpression.Value = correctValue;

                castExpression.ReplaceWith(clonedPrimitiveExpression);
                castExpression.Remove();
            }
        }
    }
}
