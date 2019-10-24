using Hast.Transformer.Helpers;
using ICSharpCode.NRefactory.CSharp;
using System;
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

                var targetFullName = invocationExpression.Target.GetFullName();

                if (targetFullName == "Microsoft.FSharp.Collections.ArrayModule.ZeroCreate")
                {
                    // Changing an expression like
                    //      ArrayModule.ZeroCreate<Task<uint>> (280)
                    // into 
                    //      new Task<uint>[280]

                    var arrayCreateExpression = new ArrayCreateExpression();

                    invocationExpression.CopyAnnotationsTo(arrayCreateExpression);
                    arrayCreateExpression.Type = ((MemberReferenceExpression)invocationExpression.Target).TypeArguments.Single().Clone();
                    arrayCreateExpression.Arguments.Add(invocationExpression.Arguments.Single().Clone());

                    invocationExpression.ReplaceWith(arrayCreateExpression);
                }
                else if (targetFullName == "Microsoft.FSharp.Collections.ArrayModule.Get" ||
                    targetFullName == "Microsoft.FSharp.Collections.ArrayModule.Set")
                {
                    var indexerExpression = new IndexerExpression();

                    var arguments = invocationExpression.Arguments.ToArray();
                    invocationExpression.CopyAnnotationsTo(indexerExpression);
                    indexerExpression.Target = arguments[0].Clone();
                    indexerExpression.Arguments.Add(arguments[1].Clone());

                    if (targetFullName == "Microsoft.FSharp.Collections.ArrayModule.Get")
                    {
                        // Changing an expression like
                        //      ArrayModule.Get<Task<uint>> (array, i)
                        // into 
                        //      array[i]

                        invocationExpression.ReplaceWith(indexerExpression);
                    }
                    else if (targetFullName == "Microsoft.FSharp.Collections.ArrayModule.Set")
                    {
                        // Changing an expression like
                        //      Array.set myArray i 5
                        // into 
                        //      array[i] = 5

                        var assignment = new AssignmentExpression(indexerExpression, arguments[2].Clone());
                        invocationExpression.CopyAnnotationsTo(assignment);
                        invocationExpression.ReplaceWith(assignment);
                    }
                }
            }
        }
    }
}
