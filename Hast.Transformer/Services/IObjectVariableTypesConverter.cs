using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts the type of variables of type <c>object</c> to the actual type they'll contain if this can be determined.
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
