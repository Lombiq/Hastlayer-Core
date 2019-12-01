using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp;
using System.Linq;

namespace Hast.Transformer.Helpers
{
    internal static class MethodDeclarationFactory
    {
        public static MethodDeclaration CreateMethod(
            string name,
            IEnumerable<object> annotations,
            AstNodeCollection<AttributeSection> attributes,
            IEnumerable<ParameterDeclaration> parameters,
            BlockStatement body,
            AstType returnType)
        {
            var method = new MethodDeclaration
            {
                Name = name
            };

            foreach (var annotation in annotations)
            {
                method.AddAnnotation(annotation);
            }

            method.Attributes.AddRange(attributes.Select(attribute => (AttributeSection)attribute.Clone()));

            method.Parameters.AddRange(parameters.Select(parameter => parameter.Clone()));

            method.Body = (BlockStatement)body.Clone();
            method.ReturnType = returnType.Clone();

            return method;
        }
    }
}
