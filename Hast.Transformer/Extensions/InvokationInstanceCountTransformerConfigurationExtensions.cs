using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Common.Configuration
{
    public static class InvokationInstanceCountTransformerConfigurationExtensions
    {
        public static MemberInvokationInstanceCountConfiguration GetMaxInvokationInstanceCountConfigurationForMember(
            this TransformerConfiguration configuration,
            EntityDeclaration entity)
        {
            var simpleName = entity.GetSimpleName();

            if (!entity.GetFullName().IsDisplayClassMemberName())
            {
                return configuration.GetMaxInvokationInstanceCountConfigurationForMember(simpleName);
            }

            // If this is a DisplayClass member then it was generated from a lamdbda expression. So need to handle it
            // with the special "MemberNamePrefix.LambdaExpression.[Index]" pattern.

            var indexedNameHolder = entity.Annotation<LambdaExpressionIndexedNameHolder>();

            // If there is no IndexedNameHolder then first we need to generate the indices for all lambdas.
            if (indexedNameHolder == null)
            {
                // Run the index-setting logic on the members of the parent class.

                var parentType = entity
                    .FindFirstParentTypeDeclaration() // The DisplayClass.
                    .FindFirstParentTypeDeclaration(); // The parent type.

                var displayClassMembers = parentType.Members
                    .Where(member => member.GetFullName().IsDisplayClassName())
                    .SelectMany(displayClass => ((TypeDeclaration)displayClass).Members)
                    .ToDictionary(displayClassMember => displayClassMember.GetFullName());

                parentType.AcceptVisitor(new IndexedNameHolderSettingVisitor(displayClassMembers));

                indexedNameHolder = entity.Annotation<LambdaExpressionIndexedNameHolder>();

                // If it's still null then the member wasn't generated from a lambda expression and thus normal rules
                // apply.
                if (indexedNameHolder == null)
                {
                    return configuration.GetMaxInvokationInstanceCountConfigurationForMember(simpleName);
                }
            }

            return configuration.GetMaxInvokationInstanceCountConfigurationForMember(indexedNameHolder.IndexedName);
        }


        private class LambdaExpressionIndexedNameHolder
        {
            public string IndexedName { get; set; }
        }

        private class IndexedNameHolderSettingVisitor : DepthFirstAstVisitor
        {
            private readonly Dictionary<string, EntityDeclaration> _displayClassMembers;
            private readonly Dictionary<EntityDeclaration, int> _lambdaCounts = new Dictionary<EntityDeclaration, int>();

            public IndexedNameHolderSettingVisitor(Dictionary<string, EntityDeclaration> displayClassMembers)
            {
                _displayClassMembers = displayClassMembers;
            }


            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);

                // Only dealing with method references.
                if (memberReferenceExpression.Annotation<MethodDefinition>() == null) return;

                var memberFullName = memberReferenceExpression.GetFullName();

                if (!memberFullName.IsDisplayClassMemberName()) return;

                var member = _displayClassMembers[memberFullName];
                if (member.Annotation<LambdaExpressionIndexedNameHolder>() == null)
                {
                    var parentMember = memberReferenceExpression.FindFirstParentOfType<EntityDeclaration>();

                    if (!_lambdaCounts.ContainsKey(parentMember))
                    {
                        _lambdaCounts[parentMember] = 0;
                    }

                    member.AddAnnotation(new LambdaExpressionIndexedNameHolder
                    {
                        IndexedName = 
                            parentMember.GetSimpleName() + 
                            ".LambdaExpression." + 
                            _lambdaCounts[parentMember].ToString()
                    });

                    _lambdaCounts[parentMember]++;
                }
            }
        }
    }
}
