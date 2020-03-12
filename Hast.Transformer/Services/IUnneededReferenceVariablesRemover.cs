using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Removes those variables from the syntax tree which are just aliases to another variable and thus unneeded. Due
    /// to reference behavior such alias variables make hardware generation much more complicated so it's better to get
    /// rid of them.
    /// </summary>
    /// <example>
    /// internal KpzKernelsTaskState <ScheduleIterations>b__9_0 (KpzKernelsTaskState rawTaskState)
    ///	{
    ///		KpzKernelsTaskState kpzKernelsTaskState;
    ///     kpzKernelsTaskState = rawTaskState;
    ///     // kpzKernelsTaskState is being used from now on everywhere so better to just use rawTaskState directly.
    ///		return kpzKernelsTaskState;
    ///	}
    ///	
    /// // The variable "random" is unneeded here.
    /// RandomMwc64X random;
    /// random = array [num4].Random1;
    /// random.State = (random.State | ((ulong)num8 << 32));
    /// </example>
    public interface IUnneededReferenceVariablesRemover : IDependency
    {
        void RemoveUnneededVariables(SyntaxTree syntaxTree);
    }
}
