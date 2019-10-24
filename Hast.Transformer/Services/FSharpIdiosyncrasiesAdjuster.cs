using ICSharpCode.NRefactory.CSharp;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class FSharpIdiosyncrasiesAdjuster : IFSharpIdiosyncrasiesAdjuster
    {
        public void AdjustFSharpIdiosyncrasie(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new FSharpIdiosyncrasiesAdjustingVisitor());
        }


        private class FSharpIdiosyncrasiesAdjustingVisitor : DepthFirstAstVisitor
        {
            public override void VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                base.VisitInvocationExpression(invocationExpression);

                // Changing an expression like
                //      ArrayModule.ZeroCreate<Task<uint>> (280)
                // into 
                //      new Task<uint>[280]

                if (invocationExpression.Target.GetFullName() != "Microsoft.FSharp.Collections.ArrayModule.ZeroCreate")
                {
                    return;
                }

                var arrayCreateExpression = new ArrayCreateExpression();

                invocationExpression.CopyAnnotationsTo(arrayCreateExpression);
                arrayCreateExpression.Type = ((MemberReferenceExpression)invocationExpression.Target).TypeArguments.Single().Clone();
                arrayCreateExpression.Arguments.Add(invocationExpression.Arguments.Single().Clone());

                invocationExpression.ReplaceWith(arrayCreateExpression);
            }
        }
    }
}
