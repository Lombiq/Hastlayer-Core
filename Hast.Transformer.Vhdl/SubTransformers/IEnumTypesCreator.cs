using System.Collections.Generic;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.Decompiler.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IEnumTypesCreator : IDependency
    {
        IEnumerable<IVhdlElement> CreateEnumTypes(SyntaxTree syntaxTree);
    }
}
