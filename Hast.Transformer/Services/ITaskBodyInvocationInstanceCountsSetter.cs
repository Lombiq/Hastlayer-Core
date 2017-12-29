using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Layer;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Configures the invocation instance count for the body delegates of <see cref="Task"/>s, i.e. determines how
    /// many hardware copies a Task's body needs. Sets the <see cref="MemberInvocationInstanceCountConfiguration"/>
    /// automatically.
    /// </summary>
    /// <example>
    /// For example in this case:
    /// 
    /// for (uint i = 0; i < 10; i++)
    /// {
    ///     tasks[i] = Task.Factory.StartNew(
    ///         indexObject =>
    ///         {
    ///             ...
    ///         },
    ///         i);
    /// }
    /// 
    /// ...this service will be able to determine that the level of parallelism is 10.
    /// </example>
    public interface ITaskBodyInvocationInstanceCountsSetter : IDependency
    {
        void SetaskBodyInvocationInstanceCounts(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
