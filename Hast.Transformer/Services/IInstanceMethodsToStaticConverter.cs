using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts instance-level class methods to static methods with an explicit object reference passed in. This is 
    /// needed so instance methods are easier to transform.
    /// </summary>
    /// <remarks>
    /// The conversion is as following:
    /// 
    /// Original method for example:
    /// <c>
    /// public uint IncreaseNumber(uint increaseBy)
    /// {
    ///     return (this.Number += increaseBy);
    /// }
    /// </c>
    /// 
    /// Will be converted into:
    /// 
    /// <c>
    /// public static uint IncreaseNumber(MyClass this, uint increaseBy)
    /// {
    ///     return (this.Number += increaseBy);
    /// }
    /// </c>
    /// 
    /// Consumer code will also be altered accordingly.
    /// </remarks>
    public interface IInstanceMethodsToStaticConverter : IDependency
    {
        void ConvertInstanceMethodsToStatic(SyntaxTree syntaxTree);
    }
}
