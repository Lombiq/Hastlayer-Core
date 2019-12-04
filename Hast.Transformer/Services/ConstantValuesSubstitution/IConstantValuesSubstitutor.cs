using Hast.Layer;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    /// <summary>
    /// Substitutes variables, fields, etc. with constants if they can only ever have a compile-time defined value.
    /// </summary>
    public interface IConstantValuesSubstitutor : IDependency
    {
        void SubstituteConstantValues(
            SyntaxTree syntaxTree,
            IArraySizeHolder arraySizeHolder,
            IHardwareGenerationConfiguration configuration);
    }
}
