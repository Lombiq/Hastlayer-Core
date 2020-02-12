using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts the type of variables of type <c>object</c> to the actual type they'll contain if this can be
    /// determined.
    /// </summary>
    /// <example>
    /// Currently the following kind of constructs are supported:
    /// 
    /// <c>
    /// // The numberObject variable will be converted to uint since apparently it is one.
    /// internal bool <ParallelizedArePrimeNumbers>b__9_0 (object numberObject)
    /// {
    ///     uint num;
    ///     num = (uint)numberObject;
    ///     // ...
    /// }
    /// </c>
    /// 
    /// Furthermore, casts to the object type corresponding to these variables when in Task starts are also removed,
    /// like the one for num4 here:
    /// <c>
    /// Task.Factory.StartNew ((Func<object, bool>)this.<ParallelizedArePrimeNumbers>b__9_0, (object)num4);
    /// </c>
    /// </example>
    /// <remarks>
    /// This is necessary because unlike an object-typed variable in .NET that due to dynamic memory allocations can
    /// hold any data in hardware the variable size should be statically determined (like fixed 32b). So compatibility
    /// with .NET object variables is not complete, thus attempting to close the loop here.
    /// </remarks>
    public interface IObjectVariableTypesConverter : IDependency
    {
        void ConvertObjectVariableTypes(SyntaxTree syntaxTree);
    }
}
