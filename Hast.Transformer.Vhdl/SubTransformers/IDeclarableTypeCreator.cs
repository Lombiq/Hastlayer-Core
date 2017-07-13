using System.Linq;
using Hast.Transformer.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Produces data types that can be used in variable, signal, etc. declarations.
    /// </summary>
    public interface IDeclarableTypeCreator : IDependency
    {
        DataType CreateDeclarableType(AstNode valueHolder, TypeReference typeReference, IVhdlTransformationContext context);
    }


    public static class DeclarableTypeCreatorExtensions
    {
        public static DataType CreateDeclarableType(
            this IDeclarableTypeCreator declarableTypeCreator,
            AstNode valueHolder,
            AstType type,
            IVhdlTransformationContext context)
        {
            var typeReference = type.GetActualTypeReference();

            if (typeReference == null)
            {
                if (type.Parent is VariableDeclarationStatement)
                {
                    typeReference = ((VariableDeclarationStatement)type.Parent).Variables.First().GetActualTypeReference();
                }
                else if (type is PrimitiveType)
                {
                    typeReference = TypeHelper.CreatePrimitiveTypeReference(((PrimitiveType)type).KnownTypeCode.ToString());
                }
                else
                {
                    typeReference = type.Parent.GetActualTypeReference();
                }
            }

            return declarableTypeCreator.CreateDeclarableType(valueHolder, typeReference, context);
        }
    }
}
