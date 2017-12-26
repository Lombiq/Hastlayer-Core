﻿using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts constructors to normal method declarations so they're easier to process later.
    /// </summary>
    public interface IConstructorsToMethodsConverter : IDependency
    {
        void ConvertConstructorsToMethods(SyntaxTree syntaxTree);
    }
}
