using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Visitors
{
    internal class InlinableTaskArraysFindingVisitor : DepthFirstAstVisitor
    {
        public Dictionary<string, string> InlinableVariableMapping { get; set; } = new Dictionary<string, string>();


        public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            base.VisitAssignmentExpression(assignmentExpression);

            // AssigmentExpression, Left = arg_*, Right.GetActualType().FullName.StartsWith("System.Threading.Tasks.Task`1<")

            var compilerGeneratedVariableName = string.Empty;
            if (assignmentExpression.Left.Is<IdentifierExpression>(identifier => 
                    (compilerGeneratedVariableName = identifier.Identifier).StartsWith("arg_")) &&
                assignmentExpression.Right.GetActualTypeReference().FullName.StartsWith("System.Threading.Tasks.Task`1<"))
            {
                InlinableVariableMapping[compilerGeneratedVariableName] = 
                    ((IdentifierExpression)assignmentExpression.Right).Identifier;
            }
        }
    }
}
