using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IEnumTypesCreator : IDependency
    {
        IEnumerable<IVhdlElement> CreateEnumTypes(SyntaxTree syntaxTree);
    }
}
