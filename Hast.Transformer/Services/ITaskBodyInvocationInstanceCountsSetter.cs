using Hast.Common.Interfaces;
using Hast.Layer;
using Hast.Transformer.Abstractions.Configuration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Threading.Tasks;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Configures the invocation instance count for the body delegates of <see cref="Task"/>s, i.e. determines how
    /// many hardware copies a Task's body needs. Sets the <see cref="MemberInvocationInstanceCountConfiguration"/>
    /// automatically.
    /// </summary>
    /// <example>
    /// <para>For example in this case:</para>
    ///
    /// <code>
    /// for (uint i = 0; i &lt; 10; i++)
    /// {
    ///     tasks[i] = Task.Factory.StartNew(
    ///         indexObject =&gt;
    ///         {
    ///             ...
    ///         },
    ///         i);
    /// }
    /// </code>
    ///
    /// <para>...this service will be able to determine that the level of parallelism is 10.</para>
    /// </example>
    public interface ITaskBodyInvocationInstanceCountsSetter : IDependency
    {
        void SetTaskBodyInvocationInstanceCounts(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
