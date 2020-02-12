using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IRecordComposer : IDependency
    {
        bool IsSupportedRecordMember(AstNode node);
        NullableRecord CreateRecordFromType(TypeDeclaration typeDeclaration, IVhdlTransformationContext context);
    }
}
