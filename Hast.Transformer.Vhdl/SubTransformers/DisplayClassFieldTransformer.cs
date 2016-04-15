using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class DisplayClassFieldTransformer : IDisplayClassFieldTransformer
    {
        private readonly ITypeConverter _typeConverter;


        public DisplayClassFieldTransformer(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        public Task<IMemberTransformerResult> Transform(FieldDeclaration field, IVhdlTransformationContext context)
        {
            return Task.Run(() =>
            {
                var fieldFullName = field.GetFullName();
                var fieldComponent = new BasicComponent(fieldFullName);

                // Nothing to do with "__this" fields of DisplayClasses that reference the parent class's object
                // like: public PrimeCalculator <>4__this;
                if (!field.Variables.Any(variable => variable.Name.EndsWith("__this")))
                {
                    var type = _typeConverter.ConvertAstType(field.ReturnType);

                    fieldComponent.GlobalVariables.Add(new Variable
                        {
                            Name = fieldFullName.ToExtendedVhdlId(),
                            DataType = type
                        });
                }

                return (IMemberTransformerResult)new MemberTransformerResult
                {
                    IsInterfaceMember = false,
                    Member = field,
                    ArchitectureComponentResults = new[]
                    {
                        new ArchitectureComponentResult
                        {
                            ArchitectureComponent = fieldComponent,
                            Body = fieldComponent.BuildBody(),
                            Declarations = fieldComponent.BuildDeclarations()
                        }
                    }
                };
            });
        }
    }
}
