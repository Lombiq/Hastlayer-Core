using System;
using System.Linq;
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

        public Record CreateRecordFromType(TypeDefinition typeDefinition)
        {
            // Process only those fields that aren't backing fields of auto-properties (since those properties are 
            // handled as properties) nor const fields (those are inserted as literals by the compiler where they're
            // used).
            var recordFields = typeDefinition.Properties
                .Cast<MemberReference>()
                .Union(typeDefinition.Fields.Where(field => !field.Name.IsBackingFieldName() && !field.HasConstant).Cast<MemberReference>())
                .Select(member =>
                {
                    var typeReference = member is FieldDefinition ?
                            ((FieldDefinition)member).FieldType :
                            ((PropertyDefinition)member).PropertyType;

                    return new RecordField
                    {
                        DataType = _typeConverterLazy.Value.ConvertTypeReference(typeReference),
                        Name = member.Name.ToExtendedVhdlId()
                    };

                })
                .ToList();

            return new Record
            {
                Name = typeDefinition.FullName.ToExtendedVhdlId(),
                Fields = recordFields
            };
        }
    }
}
