﻿using System.Collections.Generic;
using Hast.Transformer.Vhdl.Helpers;
using Hast.VhdlBuilder.Representation;
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
        
        
        public IEnumerable<ArrayType> CreateArrayTypes(SyntaxTree syntaxTree)
        {
            var arrayDeclarations = new Dictionary<string, ArrayType>();

            syntaxTree.AcceptVisitor(new ArrayCreationCheckingVisitor(_typeConverter, arrayDeclarations));

            return arrayDeclarations.Values;
        }


        private class ArrayCreationCheckingVisitor : DepthFirstAstVisitor
        {
            private readonly ITypeConverter _typeConverter;
            private readonly Dictionary<string, ArrayType> _arrayDeclarations;


            public ArrayCreationCheckingVisitor(
                ITypeConverter typeConverter,
                Dictionary<string, ArrayType> arrayDeclarations)
            {
                _typeConverter = typeConverter;
                _arrayDeclarations = arrayDeclarations;
            }


            public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                base.VisitArrayCreateExpression(arrayCreateExpression);

                var elementType = _typeConverter.ConvertAstType(arrayCreateExpression.Type);

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
