﻿using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Produces data types that can be used in variable, signal, etc. declarations.
    /// </summary>
    public interface IDeclarableTypeCreator : IDependency
    {
        DataType CreateDeclarableType(AstNode valueHolder, IType type, IVhdlTransformationContext context);
    }


    public static class DeclarableTypeCreatorExtensions
    {
        public static DataType CreateDeclarableType(
            this IDeclarableTypeCreator declarableTypeCreator,
            AstNode valueHolder,
            AstType astType,
            IVhdlTransformationContext context) =>
            declarableTypeCreator.CreateDeclarableType(valueHolder, astType.GetActualType(), context);
    }
}
