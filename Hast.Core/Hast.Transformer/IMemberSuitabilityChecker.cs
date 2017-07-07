using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer
{
    public interface IMemberSuitabilityChecker : IDependency
    {
        /// <summary>
        /// Checks whether a member is suitable to be among the hardware members that are directly executable from the 
        /// host computer.
        /// </summary>
        bool IsSuitableHardwareEntryPointMember(EntityDeclaration member, ITypeDeclarationLookupTable typeDeclarationLookupTable);
    }
}
