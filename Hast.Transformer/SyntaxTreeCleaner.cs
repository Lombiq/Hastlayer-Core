using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Transformer.Models;
using Hast.Transformer.Visitors;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer
{
    public interface ISyntaxTreeCleaner : IDependency
    {
        void Clean(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
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


        public void Clean(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration)
        {
            var typeDeclarationLookupTable = _typeDeclarationLookupTableFactory.Create(syntaxTree);
            var noIncludedMembers = !configuration.IncludedMembers.Any();
            var referencedNodesFlaggingVisitor = new ReferencedNodesFlaggingVisitor(typeDeclarationLookupTable);

            // Starting with interface members we walk through the references to see which declarations are used (e.g. which
            // methods are called at least once).
            foreach (var type in syntaxTree.GetTypes(true))
            {
                foreach (var member in type.Members)
                {
                    if ((noIncludedMembers || configuration.IncludedMembers.Contains(member.GetFullName())) &&
                        _memberSuitabilityChecker.IsSuitableInterfaceMember(member, typeDeclarationLookupTable))
                    {
                        member.IncrementReferenceCounter();
                        member.AcceptVisitor(referencedNodesFlaggingVisitor);
                    }
                }
            }

            // Then removing all unused declarations.
            syntaxTree.AcceptVisitor(new UnreferencedNodesRemovingVisitor());

            foreach (var namespaceDeclaration in syntaxTree.Members.Where(member => member is NamespaceDeclaration))
            {
                if (!((NamespaceDeclaration)namespaceDeclaration).Members.Any())
                {
                    namespaceDeclaration.Remove();
                }
            }
        }
    }
}
