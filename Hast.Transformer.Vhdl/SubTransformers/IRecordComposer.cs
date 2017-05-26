using System;
using Hast.Transformer.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IRecordComposer : IDependency
    {
        bool IsSupportedRecordMember(AstNode node);
        Record CreateRecordFromType(TypeDeclaration typeDeclaration, ITypeDeclarationLookupTable typeDeclarationLookupTable);
    }
}
