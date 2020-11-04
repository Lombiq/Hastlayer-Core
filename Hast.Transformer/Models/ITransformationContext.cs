using Hast.Layer;
using Hast.Synthesis;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Models
{
    /// <summary>
    /// The full context of a hardware transformation, including the syntax tree to transform.
    /// </summary>
    public interface ITransformationContext
    {
        /// <summary>
        /// A hash string suitable to identify the given transformation.
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

        /// <summary>
        /// Table to look up known types.
        /// </summary>
        IKnownTypeLookupTable KnownTypeLookupTable { get; }

        /// <summary>
        /// Container for the sizes of statically sized arrays.
        /// </summary>
        IArraySizeHolder ArraySizeHolder { get; }

        /// <summary>
        /// The driver of the currently targeted hardware device.
        /// </summary>
        IDeviceDriver DeviceDriver { get; }
    }
}
