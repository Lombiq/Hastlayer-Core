using ICSharpCode.Decompiler.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts customly implemented properties' setters and getters into equivalent methods so it's easier to 
    /// transform them.
    /// </summary>
    /// <example>
    /// The below property:
    /// public uint NumberPlusFive
    /// {
    ///     get { return Number + 5; }
    ///     set { Number = value - 5; }
    /// }
    /// 
    /// ...will be converted into this:
    /// uint uint get_NumberPlusFive()
    /// {
    ///     return Number + 5u;
    /// }
    ///     
    /// void void set_NumberPlusFive(uint value)
    /// {
    ///     Number = value - 5u;
    /// }
    /// </example>
    public interface ICustomPropertiesToMethodsConverter : IDependency
    {
        void ConvertCustomPropertiesToMethods(SyntaxTree syntaxTree);
    }
}
