using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Helpers
{
    public static class ExceptionHelper
    {
        public static void ThrowDeclarationNotFoundException(string typeFullName) =>
            throw new InvalidOperationException(
                "The declaration of the type " + typeFullName +
                " couldn't be found. Did you forget to add its assembly to the list of the assemblies to generate hardware from? " +
                "Or did you reference the type in a declaration (like a variable's type, or a method's return type) but never actually used any of its members and didn't instantiate it? " +
                "Remember that Hastlayer cleans up everything unused, so maybe you did use it but in a piece of code that never gets executed?");

        public static void ThrowOnlySingleDimensionalArraysSupporterException(AstNode affectedNode) =>
            throw new NotSupportedException(
                affectedNode.ToString() + " is a multi-dimensional array. " +
                "Only single-dimensional arrays are supported, see: https://github.com/Lombiq/Hastlayer-SDK/issues/10."
                .AddParentEntityName(affectedNode));
    }
}
