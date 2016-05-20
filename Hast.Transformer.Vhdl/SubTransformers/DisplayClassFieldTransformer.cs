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
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class DisplayClassFieldTransformer : IDisplayClassFieldTransformer
    {
        private readonly ITypeConverter _typeConverter;
        private readonly IArrayCreateExpressionTransformer _arrayCreateExpressionTransformer;


        public DisplayClassFieldTransformer(
            ITypeConverter typeConverter, 
            IArrayCreateExpressionTransformer arrayCreateExpressionTransformer)
        {
            _typeConverter = typeConverter;
            _arrayCreateExpressionTransformer = arrayCreateExpressionTransformer;
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


        //private class ArrayCreationCheckingVisitor : DepthFirstAstVisitor
        //{
        //    private readonly IArrayCreateExpressionTransformer _arrayCreateExpressionTransformer;
        //    private readonly Dictionary<string, IVhdlElement> _arrayDeclarations;


        //    public ArrayCreationCheckingVisitor(ITypeConverter typeConverter, Dictionary<string, IVhdlElement> arrayDeclarations)
        //    {
        //        _typeConverter = typeConverter;
        //        _arrayDeclarations = arrayDeclarations;
        //    }


        //    public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
        //    {
        //        base.VisitArrayCreateExpression(arrayCreateExpression);

        //        var elementType = _typeConverter.ConvertAstType(arrayCreateExpression.Type);

        //        if (_arrayDeclarations.ContainsKey(elementType.Name)) return;

        //        _arrayDeclarations[elementType.Name] = new ArrayType
        //        {
        //            ElementType = elementType,
        //            Name = ArrayTypeNameHelper.CreateArrayTypeName(elementType.Name)
        //        };
        //    }
        //}
    }
}
