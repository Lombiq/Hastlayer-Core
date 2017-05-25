using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.NRefactory.CSharp;

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


        public bool IsSupportedMember(AstNode node)
        {
            return 
                _recordComposer.IsSupportedRecordMember(node) || 
                (node is FieldDeclaration && !_displayClassFieldTransformer.IsDisplayClassField((FieldDeclaration)node));
        }

        public Task<IMemberTransformerResult> Transform(TypeDeclaration typeDeclaration, IVhdlTransformationContext context)
        {
            return Task.Run<IMemberTransformerResult>(() =>
            {
                var result = new MemberTransformerResult
                {
                    Member = typeDeclaration
                };

                var record = _recordComposer.CreateRecordFromType(typeDeclaration, context.TypeDeclarationLookupTable);
                var component = new BasicComponent(record.Name);

                if (record.Fields.Any())
                {
                    component.DependentTypesTable.AddBaseType(record);
                }

                result.ArchitectureComponentResults = new List<IArchitectureComponentResult>
                    {
                        {
                            new ArchitectureComponentResult
                            {
                                ArchitectureComponent = component,
                                Declarations = component.BuildDeclarations(),
                                Body = component.BuildBody()
                            }
                        }
                    };

                return result;
            });
        }
    }
}
