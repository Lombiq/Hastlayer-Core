using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts customly implemented properties' setters and getters into equivalent methods so it's easier to 
    /// transform them.
    /// </summary>
    /// <example>
    /// <code>
    /// public uint NumberPlusFive
    /// {
    ///     get { return Number + 5; }
    ///     set { Number = value - 5; }
    /// }
    /// </code>
    /// <para>The above property will be converted as below.</para>
    /// <code>
    /// uint uint get_NumberPlusFive()
    /// {
    ///     return Number + 5u;
    /// }
    ///
    /// void void set_NumberPlusFive(uint value)
    /// {
    ///     Number = value - 5u;
    /// }
    /// </code>
    /// </example>
    public interface ICustomPropertiesToMethodsConverter : IDependency
    {
        void ConvertCustomPropertiesToMethods(SyntaxTree syntaxTree);
    }
}
