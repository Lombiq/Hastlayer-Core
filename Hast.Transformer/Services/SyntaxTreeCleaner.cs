using System.Linq;
using Hast.Common.Configuration;
using Hast.Common.Extensions;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Services
{
    public class SyntaxTreeCleaner : ISyntaxTreeCleaner
    {
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;
        private readonly IMemberSuitabilityChecker _memberSuitabilityChecker;


        public SyntaxTreeCleaner(
            ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory,
            IMemberSuitabilityChecker memberSuitabilityChecker)
        {
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
            _memberSuitabilityChecker = memberSuitabilityChecker;
        }


        public void CleanUnusedDeclarations(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration)
        {
            var typeDeclarationLookupTable = _typeDeclarationLookupTableFactory.Create(syntaxTree);
            var noIncludedMembers = !configuration.HardwareEntryPointMemberFullNames.Any() && !configuration.HardwareEntryPointMemberNamePrefixes.Any();
            var referencedNodesFlaggingVisitor = new ReferencedNodesFlaggingVisitor(typeDeclarationLookupTable);

            // Starting with hardware entry point members we walk through the references to see which declarations are 
            // used (e.g. which methods are called at least once).
            foreach (var type in syntaxTree.GetAllTypeDeclarations())
            {
                foreach (var member in type.Members)
                {
                    var fullName = member.GetFullName();
                    if ((
                            (noIncludedMembers ||
                            configuration.HardwareEntryPointMemberFullNames.Contains(fullName) ||
                            fullName.GetMemberNameAlternates().Intersect(configuration.HardwareEntryPointMemberFullNames).Any() ||
                            configuration.HardwareEntryPointMemberNamePrefixes.Any(prefix => member.GetSimpleName().StartsWith(prefix)))
                        &&
                            _memberSuitabilityChecker.IsSuitableHardwareEntryPointMember(member, typeDeclarationLookupTable)
                        ))
                    {
                        if (member is MethodDeclaration)
                        {
                            var implementedInterfaceMethod = ((MethodDeclaration)member)
                                .FindImplementedInterfaceMethod(typeDeclarationLookupTable.Lookup);
                            if (implementedInterfaceMethod != null)
                            {
                                implementedInterfaceMethod.AddReference(member);
                                implementedInterfaceMethod.FindFirstParentTypeDeclaration().AddReference(member);
                            }
                        }

                        member.SetHardwareEntryPointMember();
                        member.AddReference(syntaxTree);
                        member.AcceptVisitor(referencedNodesFlaggingVisitor);
                    }
                }
            }

            // Then removing all unused declarations.
            syntaxTree.AcceptVisitor(new UnreferencedNodesRemovingVisitor());

            // Removing orphaned base types.
            foreach (var type in syntaxTree.GetAllTypeDeclarations())
            {
                foreach (var baseType in type.BaseTypes.Where(baseType => !typeDeclarationLookupTable.Lookup(baseType).IsReferenced()))
                {
                    type.BaseTypes.Remove(baseType);
                }
            }

            // Cleaning up empty namespaces.
            foreach (var namespaceDeclaration in syntaxTree.Members.Where(member => member is NamespaceDeclaration))
            {
                if (!((NamespaceDeclaration)namespaceDeclaration).Members.Any())
                {
                    namespaceDeclaration.Remove();
                }
            }

            // Note that at this point the reference counters are out of date and would need to be refreshed to be used.
        }


        private class ReferencedNodesFlaggingVisitor : DepthFirstAstVisitor
        {
            private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;


            public ReferencedNodesFlaggingVisitor(ITypeDeclarationLookupTable typeDeclarationLookupTable)
            {
                _typeDeclarationLookupTable = typeDeclarationLookupTable;
            }


            public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                base.VisitObjectCreateExpression(objectCreateExpression);

                var instantiatedType = _typeDeclarationLookupTable.Lookup(objectCreateExpression.Type);

                if (instantiatedType == null) return;

                instantiatedType.AddReference(objectCreateExpression);

                // Looking up the constructor used.
                var constructor = instantiatedType.Members
                    .SingleOrDefault(member => member.GetFullName() == objectCreateExpression.Annotation<MethodReference>().FullName);
                if (constructor != null)
                {
                    constructor.AddReference(objectCreateExpression);
                    constructor.AcceptVisitor(this);
                }
            }

            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);


                if (memberReferenceExpression.Target is TypeReferenceExpression)
                {
                    var typeReferenceExpression = (TypeReferenceExpression)memberReferenceExpression.Target;
                    if (typeReferenceExpression.Type.Is<SimpleType>(simple => simple.Identifier == "MethodImplOptions"))
                    {
                        // This can happen when a method is extern (see: https://msdn.microsoft.com/en-us/library/e59b22c5.aspx),
                        // thus has no body but has the MethodImpl attribute (e.g. Math.Abs(double value). Nothing to do.
                        return;
                    }
                }


                var member = memberReferenceExpression.GetMemberDeclaration(_typeDeclarationLookupTable);

                if (member == null || member.WasVisited()) return;

                // Using the reference expression as the "from", since e.g. two calls to the same method should be counted 
                // twice, even if from the same method.
                member.AddReference(memberReferenceExpression);

                // Referencing the member's parent as well.
                member.FindFirstParentTypeDeclaration().AddReference(memberReferenceExpression);

                // And also the interfaces implemented by it.
                if (member is MethodDeclaration || member is PropertyDeclaration)
                {
                    var implementedInterfaceMethod = member.FindImplementedInterfaceMethod(_typeDeclarationLookupTable.Lookup);
                    if (implementedInterfaceMethod != null)
                    {
                        implementedInterfaceMethod.AddReference(member);
                        implementedInterfaceMethod.FindFirstParentTypeDeclaration().AddReference(member);
                    }
                }

                member.SetVisited();

                // Since when e.g. another method is referenced that is above the level of this expression in the syntax
                // tree, thus it won't be visited unless we start a visitor there too.
                member.AcceptVisitor(this);
            }
        }

        private class UnreferencedNodesRemovingVisitor : DepthFirstAstVisitor
        {
            public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration)
            {
                base.VisitCustomEventDeclaration(eventDeclaration);
                RemoveIfUnreferenced(eventDeclaration);
            }

            public override void VisitEventDeclaration(EventDeclaration eventDeclaration)
            {
                base.VisitEventDeclaration(eventDeclaration);
                RemoveIfUnreferenced(eventDeclaration);
            }

            public override void VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration)
            {
                base.VisitDelegateDeclaration(delegateDeclaration);
                RemoveIfUnreferenced(delegateDeclaration);
            }

            public override void VisitExternAliasDeclaration(ExternAliasDeclaration externAliasDeclaration)
            {
                base.VisitExternAliasDeclaration(externAliasDeclaration);
                RemoveIfUnreferenced(externAliasDeclaration);
            }

            public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
            {
                base.VisitTypeDeclaration(typeDeclaration);

                var unreferencedMembers = typeDeclaration.Members.Where(member => !member.IsReferenced());

                if (typeDeclaration.Members.Count == unreferencedMembers.Count())
                {
                    typeDeclaration.Remove();
                }

                foreach (var member in unreferencedMembers)
                {
                    member.Remove();
                }
            }


            private static void RemoveIfUnreferenced(AstNode node)
            {
                if (!node.IsReferenced()) node.Remove();
            }
        }
    }
}
