using System;

namespace Hast.Transformer.Vhdl.Helpers
{
    internal static class ExceptionHelper
    {
        public static void ThrowDeclarationNotFoundException(string typeFullName)
        {
            throw new InvalidOperationException(
                "The declaration of the type " + typeFullName + " couldn't be found. Did you forget to add an assembly to the list of the assemblies to generate hardware from?");
        }
    }
}
