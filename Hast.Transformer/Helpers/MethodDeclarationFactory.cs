using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Helpers
{
    internal static class MethodDeclarationFactory
    {
        public static MethodDeclaration CreateMethod(
            string name,
            IEnumerable<object> annotations,
            IEnumerable<ParameterDeclaration> parameters,
            BlockStatement body,
            AstType returnType)
        {
            var method = new MethodDeclaration();

            method.Name = name;

            foreach (var annotation in annotations)
            {
                method.AddAnnotation(annotation);
            }

            foreach (var parameter in parameters)
            {
                method.Parameters.Add(parameter.Clone());
            }

            method.Body = (BlockStatement)body.Clone();
            method.ReturnType = returnType.Clone();

            return method;
        }
    }
}
