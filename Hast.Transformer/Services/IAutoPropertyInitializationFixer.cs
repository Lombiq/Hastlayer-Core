using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Fixes leftover backing field references after decompiling initialized auto-properties.
    /// </summary>
    /// <example>
    /// Consider the following property initialization:
    /// public uint Number { get; set; } = 99;
    /// 
    /// While the property itself will be restored as its original form the assigment will be added in the constructor,
    /// referencing the property's backing field:
    /// 
    /// this.<Number>k__BackingField = 99u;
    /// 
    /// The service fixes this as below:
    /// this.Number = 99u;
    /// </example>
    public interface IAutoPropertyInitializationFixer : IDependency
    {
        void FixAutoPropertyInitializations(SyntaxTree syntaxTree);
    }
}
