using Hast.Common.Helpers;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using System.Linq;

namespace Hast.Transformer.Helpers
{
    public static class VariableHelper
    {
        public static IdentifierExpression DeclareAndReferenceArrayVariable(
            Expression valueHolder,
            AstType arrayElementAstType,
            IType arrayType)
        {
            var declarationType = new ComposedType { BaseType = arrayElementAstType.Clone() };
            declarationType.ArraySpecifiers.Add(
                new ArraySpecifier(((ArrayType)arrayType).Dimensions));

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
            IType type,
            AstType astType,
            Statement parentStatement)
        {
            var variableDeclaration = new VariableDeclarationStatement(astType.Clone(), variableName)
                .WithAnnotation(new ResolveResult(type));
            variableDeclaration.Variables.Single().AddAnnotation(type);
            AstInsertionHelper.InsertStatementBefore(parentStatement, variableDeclaration);

            return new IdentifierExpression(variableName).WithAnnotation(type);
        }
    }
}
