using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IRecordComposer : IDependency
    {
        bool IsSupportedRecordMember(AstNode node);
        NullableRecord CreateRecordFromType(TypeDeclaration typeDeclaration, IVhdlTransformationContext context);
    }
}
