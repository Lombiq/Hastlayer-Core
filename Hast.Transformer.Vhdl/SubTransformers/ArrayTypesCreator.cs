using System.Collections.Generic;
using Hast.Transformer.Helpers;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.Ast;
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
            private readonly HashSet<string> _simpleMemoryArrayTypeNames = new HashSet<string>();


            public ArrayCreationCheckingVisitor(
                ITypeConverter typeConverter,
                Dictionary<string, ArrayType> arrayDeclarations, 
                IVhdlTransformationContext context)
            {
                _typeConverter = typeConverter;
                _arrayDeclarations = arrayDeclarations;
                _context = context;

                // Excluding array types already added by the SimpleMemory package.
                if (context.GetTransformerConfiguration().UseSimpleMemory)
                {
                    _simpleMemoryArrayTypeNames.Add(ArrayHelper.CreateArrayTypeName(KnownDataTypes.Int32));
                    _simpleMemoryArrayTypeNames.Add(ArrayHelper.CreateArrayTypeName(KnownDataTypes.UInt32));
                    _simpleMemoryArrayTypeNames.Add(ArrayHelper.CreateArrayTypeName(KnownDataTypes.Boolean));
                }
            }


            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                if (SimpleMemoryAssignmentHelper.IsRead4BytesAssignment(assignmentExpression) ||
                    SimpleMemoryAssignmentHelper.IsBatchedReadAssignment(assignmentExpression, out var cellCount))
                {
                    CreateArrayDeclarationIfNew(AstBuilder.ConvertType(assignmentExpression.GetActualTypeReference().GetElementType()));
                }
            }

            public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                base.VisitArrayCreateExpression(arrayCreateExpression);

                CreateArrayDeclarationIfNew(arrayCreateExpression.GetElementType());
            }


            private void CreateArrayDeclarationIfNew(AstType elementAstType)
            {
                var elementType = _typeConverter.ConvertAstType(elementAstType, _context);

                var typeName = ArrayHelper.CreateArrayTypeName(elementType);

                if (_arrayDeclarations.ContainsKey(typeName) || _simpleMemoryArrayTypeNames.Contains(typeName)) return;

                _arrayDeclarations[typeName] = new ArrayType
                {
                    ElementType = elementType,
                    Name = typeName
                };
            }
        }
    }
}
