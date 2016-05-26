using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Helpers;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class ArrayTypesCreator : IArrayTypesCreator
    {
        private readonly ITypeConverter _typeConverter;


        public ArrayTypesCreator(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }
        
        
        public IEnumerable<IVhdlElement> CreateArrayTypes(SyntaxTree syntaxTree)
        {
            var arrayDeclarations = new Dictionary<string, IVhdlElement>();

            syntaxTree.AcceptVisitor(new ArrayCreationCheckingVisitor(_typeConverter, arrayDeclarations));

            return arrayDeclarations.Values;
        }


        private class ArrayCreationCheckingVisitor : DepthFirstAstVisitor
        {
            private readonly ITypeConverter _typeConverter;
            private readonly Dictionary<string, IVhdlElement> _arrayDeclarations;


            public ArrayCreationCheckingVisitor(
                ITypeConverter typeConverter,
                Dictionary<string, IVhdlElement> arrayDeclarations)
            {
                _typeConverter = typeConverter;
                _arrayDeclarations = arrayDeclarations;
            }


            public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                base.VisitArrayCreateExpression(arrayCreateExpression);

                var elementAstType = arrayCreateExpression.Type;
                if (elementAstType.IsTask())
                {
                    elementAstType = elementAstType.GetStoredTypeOfTaskResultArray();
                }
                var elementType = _typeConverter.ConvertAstType(elementAstType);

                if (_arrayDeclarations.ContainsKey(elementType.Name)) return;

                _arrayDeclarations[elementType.Name] = new ArrayType
                {
                    ElementType = elementType,
                    Name = ArrayTypeNameHelper.CreateArrayTypeName(elementType.Name)
                };
            }
        }
    }
}
