using Hast.Transformer.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;
using System.Linq;

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
            AstType astType,
            IVhdlTransformationContext context)
        {
            var type = astType.GetActualType();

            if (type == null)
            {
                if (astType.Parent is VariableDeclarationStatement)
                {
                    type = ((VariableDeclarationStatement)astType.Parent).Variables.First().GetActualType();
                }
                else if (astType is PrimitiveType)
                {
                    type = TypeHelper.CreatePrimitiveTypeReference(((PrimitiveType)astType).KnownTypeCode.ToString());
                }
                else
                {
                    type = astType.Parent.GetActualType();
                }
            }

            return declarableTypeCreator.CreateDeclarableType(valueHolder, type, context);
        }
    }
}
