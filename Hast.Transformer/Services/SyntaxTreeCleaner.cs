using System.Linq;
using Hast.Common.Configuration;
using Hast.Common.Extensions;
using Hast.Transformer.Models;
using Hast.Transformer.Visitors;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Removes nodes from the syntax tree that aren't needed.
    /// </summary>
    public interface ISyntaxTreeCleaner : IDependency
    {
        void CleanUnusedDeclarations(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }


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
            var noIncludedMembers = !configuration.PublicHardwareMemberFullNames.Any() && !configuration.PublicHardwareMemberNamePrefixes.Any();
            var referencedNodesFlaggingVisitor = new ReferencedNodesFlaggingVisitor(typeDeclarationLookupTable);

            // Starting with interface members we walk through the references to see which declarations are used (e.g. 
            // which methods are called at least once).
            foreach (var type in syntaxTree.GetAllTypeDeclarations())
            {
                foreach (var member in type.Members)
                {
                    var fullName = member.GetFullName();
                    if (
                            (noIncludedMembers || 
                            configuration.PublicHardwareMemberFullNames.Contains(fullName) || 
                            fullName.GetMemberNameAlternates().Intersect(configuration.PublicHardwareMemberFullNames).Any() || 
                            configuration.PublicHardwareMemberNamePrefixes.Any(prefix => member.GetSimpleName().StartsWith(prefix))) 
                        &&
                            _memberSuitabilityChecker.IsSuitableInterfaceMember(member, typeDeclarationLookupTable))
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

                        member.SetInterfaceMember();
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
    }
}
