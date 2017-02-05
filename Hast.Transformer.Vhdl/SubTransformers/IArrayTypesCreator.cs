using System.Collections.Generic;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IArrayTypesCreator : IDependency
    {
        IEnumerable<ArrayType> CreateArrayTypes(SyntaxTree syntaxTree);
    }
}
