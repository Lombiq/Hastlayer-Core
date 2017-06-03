using System;
using System.Linq;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard.Caching;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class RecordComposer : IRecordComposer
    {
        // Needs Lazy because unfortunately TypeConverter and RecordComposer depend on each other.
        private readonly Lazy<ITypeConverter> _typeConverterLazy;
        private readonly ICacheManager _cacheManager;


        public RecordComposer(Lazy<ITypeConverter> typeConverterLazy, ICacheManager cacheManager)
        {
            _typeConverterLazy = typeConverterLazy;
            _cacheManager = cacheManager;
        }


        public bool IsSupportedRecordMember(AstNode node)
        {
            return node is PropertyDeclaration || node is FieldDeclaration;
        }

        public Record CreateRecordFromType(TypeDeclaration typeDeclaration, ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            // Using transient caching because when processing an assembly all references to a class or struct will 
            // result in the record being composed.
            return _cacheManager.Get("ComposedRecord." + typeDeclaration.GetFullName(), true, context =>
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
                        ArrayCreateExpression arrayCreateExpression = null;

                        if (member is FieldDeclaration)
                        {
                            var variable = ((FieldDeclaration)member).Variables.Single();
                            name = variable.Name;
                            arrayCreateExpression = variable.Initializer as ArrayCreateExpression;
                        }

                        DataType type = null;
                        if (member.ReturnType.IsArray())
                        {
                            var arrayLength = arrayCreateExpression != null ?
                                arrayCreateExpression.GetStaticLength() :
                                member.Annotation<ConstantArrayLength>().Length;

                            type = ArrayHelper.CreateArrayInstantiation(
                                _typeConverterLazy.Value.ConvertAstType(
                                    ((ComposedType)member.ReturnType).BaseType,
                                    typeDeclarationLookupTable),
                                arrayLength);
                        }
                        else
                        {
                            type = _typeConverterLazy.Value.ConvertAstType(member.ReturnType, typeDeclarationLookupTable);
                        }

                        return new RecordField
                        {
                            DataType = type,
                            Name = name.ToExtendedVhdlId()
                        };

                    })
                    .ToList();

                return new Record
                {
                    Name = typeDeclaration.GetFullName().ToExtendedVhdlId(),
                    Fields = recordFields
                };
            });
        }
    }
}
