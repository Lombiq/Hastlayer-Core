using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;

namespace Hast.Transformer.Helpers
{
    internal static class VariableHelper
    {
        public static IdentifierExpression DeclareAndReferenceArrayVariable(
            Expression valueHolder,
            AstType arrayElementAstType,
            TypeReference arrayType)
        {
            var declarationType = new ComposedType { BaseType = arrayElementAstType.Clone() };
            declarationType.ArraySpecifiers.Add(
                new ArraySpecifier(((Mono.Cecil.ArrayType)arrayType).Dimensions.Count));

            return DeclareAndReferenceVariable("array", valueHolder, declarationType);
        }

        public static IdentifierExpression DeclareAndReferenceVariable(
            string variableNamePrefix,
            Expression valueHolder,
            AstType type)
        {
            var variableName = variableNamePrefix + Sha2456Helper.ComputeHash(valueHolder.GetFullName());
            var parentStatement = valueHolder.FindFirstParentStatement();
            var typeInformation = valueHolder.Annotation<TypeInformation>();

            var variableDeclaration = new VariableDeclarationStatement(type.Clone(), variableName)
                .WithAnnotation(typeInformation);
            variableDeclaration.Variables.Single().AddAnnotation(typeInformation);
            AstInsertionHelper.InsertStatementBefore(parentStatement, variableDeclaration);

            return new IdentifierExpression(variableName).WithAnnotation(typeInformation);
        }
    }
}
