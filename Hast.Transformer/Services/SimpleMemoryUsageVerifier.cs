using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    public class SimpleMemoryUsageVerifier : ISimpleMemoryUsageVerifier
    {
        public void VerifySimpleMemoryUsage(SyntaxTree syntaxTree)
        {
            foreach (var type in syntaxTree.GetAllTypeDeclarations())
            {
                foreach (var member in type.Members.Where(m => m.IsHardwareEntryPointMember()))
                {
                    if (member is MethodDeclaration method)
                    {
                        var methodName = member.GetFullName();

                        if (string.IsNullOrEmpty(method.GetSimpleMemoryParameterName()))
                        {
                            throw new InvalidOperationException(
                                "The method " + methodName + " doesn't have a necessary SimpleMemory parameter. Hardware entry points should have one.");
                        }

                        if (method.Parameters.Count > 1)
                        {
                            throw new InvalidOperationException(
                                "The method " + methodName + " contains parameters apart from the SimpleMemory parameter. Hardware entry points should only have a single SimpleMemory parameter and nothing else.");
                        }
                    }
                }
            }

            syntaxTree.AcceptVisitor(new SimpleMemoryAssignmentVerifyingVisitor());
        }

        private class SimpleMemoryAssignmentVerifyingVisitor : DepthFirstAstVisitor
        {
            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                if (SimpleMemoryAssignmentHelper.IsBatchedReadAssignment(assignmentExpression, out var cellCount) &&
                    cellCount <= 0)
                {
                    throw new NotSupportedException(
                        "The cell count parameter of the batched SimpleMemory read operation in the expression " +
                        assignmentExpression.ToString() +
                        " couldn't be statically determined. Such operations need to define the cell count at compile-time."
                        .AddParentEntityName(assignmentExpression));
                }
            }
        }
    }
}