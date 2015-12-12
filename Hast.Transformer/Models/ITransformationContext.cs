﻿using Hast.Common.Configuration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Models
{
    /// <summary>
    /// The full context of a hardware transformation, including the syntax tree to transform.
    /// </summary>
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
        /// Table to look up type declarations in the syntax tree.
        /// </summary>
        ITypeDeclarationLookupTable TypeDeclarationLookupTable { get; }
    }
}
