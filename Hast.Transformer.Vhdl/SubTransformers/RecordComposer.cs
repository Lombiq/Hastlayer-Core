using System;
using System.Linq;
using Hast.Transformer.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class RecordComposer : IRecordComposer
    {
        // Needs Lazy because unfortunately TypeConverter and RecordComposer depend on each other.
        private readonly Lazy<ITypeConverter> _typeConverterLazy;


        public RecordComposer(Lazy<ITypeConverter> typeConverterLazy)
        {
            _typeConverterLazy = typeConverterLazy;
        }


        public bool IsSupportedRecordMember(AstNode node)
        {
            return node is PropertyDeclaration || node is FieldDeclaration;
        }

        public Record CreateRecordFromType(TypeDeclaration typeDeclaration, ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            // Process only those fields that aren't backing fields of auto-properties (since those properties are 
            // handled as properties) nor const fields (those are inserted as literals by the compiler where they're
            // used).
            var recordFields = typeDeclaration.Members
                .Where(member => 
                    member is PropertyDeclaration || 
                    member.Is<FieldDeclaration>(field => !field.GetFullName().IsBackingFieldName()))
                .Select(member =>
                {
                    var name = member.Name;

                    if (member is FieldDeclaration)
                    {
                        name = ((FieldDeclaration)member).Variables.Single().Name;
                    }

                    return new RecordField
                    {
                        DataType = _typeConverterLazy.Value.ConvertAstType(member.ReturnType, typeDeclarationLookupTable),
                        Name = name.ToExtendedVhdlId()
                    };

                })
                .ToList();

            return new Record
            {
                Name = typeDeclaration.GetFullName().ToExtendedVhdlId(),
                Fields = recordFields
            };
        }
    }
}
