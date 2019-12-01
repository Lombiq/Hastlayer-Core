using System.Linq;
using Hast.Common.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Helpers
{
    public static class VariableHelper
    {
        public static IdentifierExpression DeclareAndReferenceArrayVariable(
            Expression valueHolder,
            AstType arrayElementAstType,
            TypeReference arrayType)
        {
            var declarationType = new ComposedType { BaseType = arrayElementAstType.Clone() };
            declarationType.ArraySpecifiers.Add(
                new ArraySpecifier(((ArrayType)arrayType).Dimensions.Count));

            return DeclareAndReferenceVariable("array", valueHolder, declarationType);
        }

        public static IdentifierExpression DeclareAndReferenceVariable(
            string variableNamePrefix,
            Expression valueHolder,
            AstType type)
        {
            var variableName = variableNamePrefix + Sha2456Helper.ComputeHash(valueHolder.GetFullName());
            var parentStatement = valueHolder.FindFirstParentStatement();
            var typeInformation = valueHolder.GetTypeInformationOrCreateFromActualTypeReference();

            return DeclareAndReferenceVariable(variableName, typeInformation, type, parentStatement);
        }

        public static IdentifierExpression DeclareAndReferenceVariable(
            string variableName,
            TypeInformation typeInformation,
            AstType type,
            Statement parentStatement)
        {
            var variableDeclaration = new VariableDeclarationStatement(type.Clone(), variableName)
                .WithAnnotation(typeInformation);
            variableDeclaration.Variables.Single().AddAnnotation(typeInformation);
            AstInsertionHelper.InsertStatementBefore(parentStatement, variableDeclaration);

            return new IdentifierExpression(variableName).WithAnnotation(typeInformation);
        }
    }
}
