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
            return node is PropertyDeclaration;
        }

        public Record CreateRecordFromType(TypeDefinition typeDefinition)
        {
            return new Record
            {
                Name = typeDefinition.FullName.ToExtendedVhdlId(),
                Fields = typeDefinition.Properties.Select(property => new RecordField
                {
                    DataType = _typeConverterLazy.Value.ConvertTypeReference(property.PropertyType),
                    Name = property.Name.ToExtendedVhdlId()
                }).ToList()
            };
        }
    }
}
