using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class CompilerGeneratedClassesVerifier : ICompilerGeneratedClassesVerifier
    {
        public void VerifyCompilerGeneratedClasses(SyntaxTree syntaxTree)
        {
            var compilerGeneratedClasses = syntaxTree
                .GetAllTypeDeclarations()
                .Where(type => type.ClassType == ClassType.Class && type.Name.Contains("__DisplayClass"))
                .Where(type => type
                    .Attributes
                    .Any(attributeSection => attributeSection
                        .Attributes.Any(attribute => attribute.Type.GetSimpleName() == "CompilerGeneratedAttribute")));

            foreach (var compilerGeneratedClass in compilerGeneratedClasses)
            {
                var fields = compilerGeneratedClass.Members.OfType<FieldDeclaration>()
                    .OrderBy(field => field.Variables.Single().Name)
                    .ToDictionary(field => field.Variables.Single().Name);

                foreach (var method in compilerGeneratedClass.Members.OfType<MethodDeclaration>())
                {
                    // Adding parameters for every field that the method used, in alphabetical order, and changing field
                    // references to parameter references.

                    var parametersForFormerFields = new SortedList<string, ParameterDeclaration>();

                    Action<MemberReferenceExpression> memberReferenceExpressionProcessor = memberReferenceExpression =>
                    {
                        var fieldDefinition = memberReferenceExpression.Annotation<FieldDefinition>();

                        if (fieldDefinition == null) return;

                        var field = fields.Values
                            .Single(f => f.Annotation<FieldDefinition>().FullName == fieldDefinition.FullName);

                        // Is the field assigned to? Because we don't support that currently, since with it being
                        // converted to a parameter we'd need to return its value and assign it to the caller's
                        // variable. Maybe we'll allow this with static field support, but not for lambdas used
                        // in parallelized expressions (since that would require concurrent access too).
                        var isAssignedTo =
                            // The field is directly assigned to.
                            (memberReferenceExpression.Parent is AssignmentExpression &&
                            ((AssignmentExpression)memberReferenceExpression.Parent).Left == memberReferenceExpression)
                            ||
                            // The field's indexed element is assigned to.
                            (memberReferenceExpression.Parent is IndexerExpression &&
                            memberReferenceExpression.Parent.Parent is AssignmentExpression &&
                            ((AssignmentExpression)memberReferenceExpression.Parent.Parent).Left == memberReferenceExpression.Parent);
                        if (isAssignedTo)
                        {
                            throw new NotSupportedException("It's not supported to modify the content of a variable coming from the parent scope in a lambda expression. Pass arguments instead.");
                        }
                    };

                    method.AcceptVisitor(new MemberReferenceExpressionVisitingVisitor(memberReferenceExpressionProcessor));
                }
            }
        }


        private class MemberReferenceExpressionVisitingVisitor : DepthFirstAstVisitor
        {
            private readonly Action<MemberReferenceExpression> _expressionProcessor;


            public MemberReferenceExpressionVisitingVisitor(Action<MemberReferenceExpression> expressionProcessor)
            {
                _expressionProcessor = expressionProcessor;
            }


            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);
                _expressionProcessor(memberReferenceExpression);
            }
        }
    }
}
