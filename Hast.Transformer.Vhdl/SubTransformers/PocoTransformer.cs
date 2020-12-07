using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class PocoTransformer : IPocoTransformer
    {
        private readonly IRecordComposer _recordComposer;
        private readonly IDisplayClassFieldTransformer _displayClassFieldTransformer;

        public PocoTransformer(IRecordComposer recordComposer, IDisplayClassFieldTransformer displayClassFieldTransformer)
        {
            _recordComposer = recordComposer;
            _displayClassFieldTransformer = displayClassFieldTransformer;
        }

        public bool IsSupportedMember(AstNode node) =>
            _recordComposer.IsSupportedRecordMember(node) ||
            (node is FieldDeclaration && !_displayClassFieldTransformer.IsDisplayClassField((FieldDeclaration)node));

        public Task<IMemberTransformerResult> TransformAsync(TypeDeclaration typeDeclaration, IVhdlTransformationContext context) =>
            Task.Run<IMemberTransformerResult>(() =>
            {
                var result = new MemberTransformerResult
                {
                    Member = typeDeclaration,
                };

                var record = _recordComposer.CreateRecordFromType(typeDeclaration, context);
                var component = new BasicComponent(record.Name);

                if (record.Fields.Any())
                {
                    var hasDependency = false;

                    foreach (var field in record.Fields)
                    {
                        if (field.DataType is ArrayTypeBase || field.DataType is Record)
                        {
                            component.DependentTypesTable.AddDependency(record, field.DataType.Name);
                            hasDependency = true;
                        }
                    }

                    if (!hasDependency) component.DependentTypesTable.AddBaseType(record);
                }

                result.ArchitectureComponentResults = new List<IArchitectureComponentResult>
                {
                    {
                        new ArchitectureComponentResult
                        {
                            ArchitectureComponent = component,
                            Declarations = component.BuildDeclarations(),
                            Body = component.BuildBody(),
                        }
                    },
                };

                return result;
            });
    }
}
