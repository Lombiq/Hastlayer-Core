using System;
using System.Linq;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Mono.Cecil;
using Orchard.Caching;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class RecordComposer : IRecordComposer
    {
        // Needs Lazy because unfortunately TypeConverter and RecordComposer depend on each other.
        private readonly Lazy<IDeclarableTypeCreator> _declarableTypeCreatorLazy;
        private readonly ICacheManager _cacheManager;


        public RecordComposer(ICacheManager cacheManager, Lazy<IDeclarableTypeCreator> declarableTypeCreatorLazy)
        {
            _cacheManager = cacheManager;
            _declarableTypeCreatorLazy = declarableTypeCreatorLazy;
        }


        public bool IsSupportedRecordMember(AstNode node)
        {
            return node is PropertyDeclaration || node is FieldDeclaration;
        }

        public NullableRecord CreateRecordFromType(TypeDeclaration typeDeclaration, IVhdlTransformationContext context)
        {
            // Using transient caching because when processing an assembly all references to a class or struct will 
            // result in the record being composed.
            var typeFullName = typeDeclaration.GetFullName();

            return _cacheManager.Get("ComposedRecord." + typeFullName, true, ctx =>
            {
                var recordName = typeFullName.ToExtendedVhdlId();

                // Process only those fields that aren't backing fields of auto-properties (since those properties are 
                // handled as properties).
                var recordFields = typeDeclaration.Members
                    .Where(member =>
                        (member is PropertyDeclaration ||
                        member.Is<FieldDeclaration>(field => !field.GetFullName().IsBackingFieldName())) &&
                        !member.GetActualTypeReference().IsSimpleMemory())
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

                        DataType fieldDataType;

                        // If the field stores an instance of this type then we shouldn't declare that, otherwise we'd
                        // get a stack overflow. This won't help against having a type that contains this type, so 
                        // indirect circular type dependency.
                        if (member.ReturnType.GetFullName() == typeFullName)
                        {
                            fieldDataType = new NullableRecord { Name = recordName }.ToReference();
                        }
                        else
                        {
                            fieldDataType = _declarableTypeCreatorLazy.Value.CreateDeclarableType(member, member.ReturnType, context);
                        }

                        return new RecordField
                        {
                            DataType = fieldDataType,
                            Name = name.ToExtendedVhdlId()
                        };

                    });

                return RecordHelper.CreateNullableRecord(recordName, recordFields);
            });
        }
    }
}
