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

        public Record CreateRecordFromType(TypeDeclaration typeDeclaration, IVhdlTransformationContext context)
        {
            // Using transient caching because when processing an assembly all references to a class or struct will 
            // result in the record being composed.
            return _cacheManager.Get("ComposedRecord." + typeDeclaration.GetFullName(), true, ctx =>
            {
                // Process only those fields that aren't backing fields of auto-properties (since those properties are 
                // handled as properties).
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

                        return new RecordField
                        {
                            DataType = _declarableTypeCreatorLazy.Value.CreateDeclarableType(member, member.ReturnType, context),
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
