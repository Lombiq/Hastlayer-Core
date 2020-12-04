using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts the type of variables of type <c>object</c> to the actual type they'll contain if this can be
    /// determined.
    /// </summary>
    /// <example>
    /// <para>Currently the following kind of constructs are supported:</para>
    ///
    /// <code>
    /// // The numberObject variable will be converted to uint since apparently it is one.
    /// internal bool &lt;ParallelizedArePrimeNumbers&gt;b__9_0 (object numberObject)
    /// {
    ///     uint num;
    ///     num = (uint)numberObject;
    ///     // ...
    /// }
    /// </code>
    ///
    /// <para>
    /// Furthermore, casts to the object type corresponding to these variables when in Task starts are also removed,
    /// like the one for num4 here:
    /// </para>
    /// <code>
    /// Task.Factory.StartNew ((Func&lt;object, bool&gt;)this.&lt;ParallelizedArePrimeNumbers&gt;b__9_0, (object)num4);
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>This is necessary because unlike an object-typed variable in .NET that due to dynamic memory allocations can
    /// hold any data in hardware the variable size should be statically determined (like fixed 32b). So compatibility
    /// with .NET object variables is not complete, thus attempting to close the loop here.</para>
    /// </remarks>
    public interface IObjectVariableTypesConverter : IDependency
    {
        void ConvertObjectVariableTypes(SyntaxTree syntaxTree);
    }
}
