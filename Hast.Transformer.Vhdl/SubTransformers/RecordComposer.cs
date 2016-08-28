using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;
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
