using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Models
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
        /// Table to look up type declarations in the syntax tree.
        /// </summary>
        ITypeDeclarationLookupTable TypeDeclarationLookupTable { get; }
    }
}
