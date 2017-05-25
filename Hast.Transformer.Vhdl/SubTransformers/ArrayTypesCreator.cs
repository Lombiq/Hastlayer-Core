using System.Collections.Generic;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class ArrayTypesCreator : IArrayTypesCreator
    {
        private readonly ITypeConverter _typeConverter;


        public ArrayTypesCreator(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        public IEnumerable<ArrayType> CreateArrayTypes(SyntaxTree syntaxTree, IVhdlTransformationContext context)
        {
            var arrayDeclarations = new Dictionary<string, ArrayType>();

            syntaxTree.AcceptVisitor(new ArrayCreationCheckingVisitor(_typeConverter, arrayDeclarations, context));

            return arrayDeclarations.Values;
        }


        private class ArrayCreationCheckingVisitor : DepthFirstAstVisitor
        {
            private readonly ITypeConverter _typeConverter;
            private readonly Dictionary<string, ArrayType> _arrayDeclarations;
            private readonly IVhdlTransformationContext _context;


            public ArrayCreationCheckingVisitor(
                ITypeConverter typeConverter,
                Dictionary<string, ArrayType> arrayDeclarations, 
                IVhdlTransformationContext context)
            {
                _typeConverter = typeConverter;
                _arrayDeclarations = arrayDeclarations;
                _context = context;
            }


            public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                base.VisitArrayCreateExpression(arrayCreateExpression);

                var elementType = _typeConverter
                    .ConvertAstType(arrayCreateExpression.GetElementType(), _context.TypeDeclarationLookupTable);

                if (_arrayDeclarations.ContainsKey(elementType.Name)) return;

                _arrayDeclarations[elementType.Name] = new ArrayType
                {
                    ElementType = elementType,
                    Name = ArrayHelper.CreateArrayTypeName(elementType.Name)
                };
            }
        }
    }
}
