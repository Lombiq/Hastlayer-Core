using System;
using System.Linq;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;

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


        public bool IsDisplayClassField(FieldDeclaration field)
        {
            return field.GetFullName().IsDisplayClassMemberName();
        }

        public Task<IMemberTransformerResult> Transform(FieldDeclaration field, IVhdlTransformationContext context)
        {
            return Task.Run(() =>
            {
                var fieldFullName = field.GetFullName();
                var fieldComponent = new BasicComponent(fieldFullName);


                var shouldTransform =
                    // Nothing to do with "__this" fields of DisplayClasses that reference the parent class's object
                    // like: public PrimeCalculator <>4__this;
                    !field.Variables.Any(variable => variable.Name.EndsWith("__this")) &&
                    // Roslyn adds a field like public Func<object, bool> <>9__0; with the same argument and return types 
                    // as the original lambda. Nothing needs to be done with this.
                    !(field.ReturnType.Is<SimpleType>(simple => simple.Identifier == "Func")) &&
                    // Sometimes the compiler adds a static field containing an object of the parent class as below:
                    // public static readonly HastlayerOptimizedAlgorithm.<>c <>9 = new HastlayerOptimizedAlgorithm.<>c ();
                    field.Modifiers != (Modifiers.Public | Modifiers.Static | Modifiers.Readonly);
                if (shouldTransform)
                {
                    var dataType = _typeConverter.ConvertAstType(field.ReturnType);

                    // The field is an array so need to instantiate it.
                    if (field.ReturnType.IsArray())
                    {
                        var visitor = new ArrayCreationDataTypeRetrievingVisitor(
                            fieldFullName,
                            _arrayCreateExpressionTransformer);

                        context.SyntaxTree.AcceptVisitor(visitor);

                        dataType = visitor.ArrayDataType;
                    }

                    fieldComponent.GlobalVariables.Add(new Variable
                    {
                        Name = fieldFullName.ToExtendedVhdlId(),
                        DataType = dataType
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


        private class ArrayCreationDataTypeRetrievingVisitor : DepthFirstAstVisitor
        {
            private readonly string _fieldFullName;
            private readonly IArrayCreateExpressionTransformer _arrayCreateExpressionTransformer;

            public DataType ArrayDataType { get; private set; }


            public ArrayCreationDataTypeRetrievingVisitor(
                string fieldDefinitionFullName,
                IArrayCreateExpressionTransformer arrayCreateExpressionTransformer)
            {
                _fieldFullName = fieldDefinitionFullName;
                _arrayCreateExpressionTransformer = arrayCreateExpressionTransformer;
            }


            public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                if (ArrayDataType != null) return;

                base.VisitArrayCreateExpression(arrayCreateExpression);

                var isSearchedFieldAccess = arrayCreateExpression.Parent
                    .Is<AssignmentExpression>(assignment => assignment.Left
                        .Is<MemberReferenceExpression>(memberReference => memberReference.GetFullName() == _fieldFullName));
                if (isSearchedFieldAccess)
                {
                    ArrayDataType = _arrayCreateExpressionTransformer.CreateArrayInstantiation(arrayCreateExpression);
                }
            }
        }
    }
}
