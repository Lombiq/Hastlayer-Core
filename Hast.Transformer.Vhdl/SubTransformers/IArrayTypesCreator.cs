using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IArrayTypesCreator : IDependency
    {
        IEnumerable<ArrayType> CreateArrayTypes(SyntaxTree syntaxTree, IVhdlTransformationContext context);
    }
}
