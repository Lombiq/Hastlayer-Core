using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer
{
    public interface ITransformationContext
    {
        /// <summary>
        /// A string suitable to identify the given transformation.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The syntax tree of the code to transform.
        /// </summary>
        SyntaxTree SyntaxTree { get; }

        /// <summary>
        /// Configuration for how the hardware generation should happen.
        /// </summary>
        IHardwareGenerationConfiguration HardwareGenerationConfiguration { get; }

        /// <summary>
        /// Retrieves the type declaration, given an AST type.
        /// </summary>
        /// <param name="type">The AST type to look up the type declaration for.</param>
        /// <returns>The retrieved <see cref="TypeDeclaration"/> if found or <c>null</c> otherwise.</returns>
        TypeDeclaration LookupDeclaration(AstType type);
    }


    public static class TransformationContextExtensions
    {
        public static TypeDeclaration LookupDeclaration(this ITransformationContext transformationContext, TypeReferenceExpression expression)
        {
            return transformationContext.LookupDeclaration(expression.Type);
        }
    }
}
