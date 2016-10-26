using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class PocoTransformer : IPocoTransformer
    {
        private readonly IRecordComposer _recordComposer;


        public PocoTransformer(IRecordComposer recordComposer)
        {
            _recordComposer = recordComposer;
        }


        public Task<IMemberTransformerResult> Transform(TypeDeclaration typeDeclaration, IVhdlTransformationContext context)
        {
            return Task.Run<IMemberTransformerResult>(() =>
            {
                var result = new MemberTransformerResult
                {
                    Member = typeDeclaration
                };

                var record = _recordComposer.CreateRecordFromType(typeDeclaration);
                var component = new BasicComponent(record.Name);

                if (record.Fields.Any())
                {
                    component.Declarations = record;
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
