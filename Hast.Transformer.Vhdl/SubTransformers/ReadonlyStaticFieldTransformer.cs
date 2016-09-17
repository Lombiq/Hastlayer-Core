using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class ReadonlyStaticFieldTransformer : IReadonlyStaticFieldTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly IArrayCreateExpressionTransformer _arrayCreateExpressionTransformer;


        public ReadonlyStaticFieldTransformer(
            ITypeConverter typeConverter,
            IArrayCreateExpressionTransformer arrayCreateExpressionTransformer)
        {
            _typeConverter = typeConverter;
            _arrayCreateExpressionTransformer = arrayCreateExpressionTransformer;
        }


        public bool CanTransform(FieldDeclaration field)
        {
            return field.Modifiers.HasFlag(Modifiers.Static) && field.Modifiers.HasFlag(Modifiers.Readonly);
        }

        public Task<IMemberTransformerResult> Transform(FieldDeclaration field, IVhdlTransformationContext context)
        {
            return Task.Run(() =>
            {
                var fieldFullName = field.GetFullName();
                var fieldComponent = new BasicComponent(fieldFullName);
                var dataType = _typeConverter.ConvertAstType(field.ReturnType);
                Value value = null;

                if (!field.Variables.Any())
                {
                    throw new NotSupportedException(
                        "Only static readonly fields that have their value directly assigned in the declaration are supported.");
                }

                // The field is an array so need to instantiate it.
                if (field.ReturnType.IsArray())
                {
                    var initializer = (ArrayCreateExpression)field.Variables.Single().Initializer;
                    value = _arrayCreateExpressionTransformer.Transform(initializer, fieldComponent);
                    dataType = value.DataType;
                }
                else
                {

                }

                fieldComponent.GlobalVariables.Add(new Variable
                {
                    Name = fieldFullName.ToExtendedVhdlId(),
                    DataType = dataType,
                    InitialValue = value
                });

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
