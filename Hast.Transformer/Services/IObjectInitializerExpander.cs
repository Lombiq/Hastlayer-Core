using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts inline object initializers into one-by-one property assignments so these can be transformed in a
    /// simpler way.
    /// </summary>
    /// <example>
    /// <code>
    /// var x = new MyClass { Property1 = value1, Property2 = value2 };
    ///
    /// will be converted to:
    ///
    /// var x = new MyClass();
    /// x.Property1 = value1;
    /// x.Property2 = value2;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>There is the ObjectOrCollectionInitializers decompiler option with a similar aim. However, that would unpack
    /// initializations for compiler-generated methods created from closures and processing that would be painful. 
    /// Also, with that option a new variable is created for every instantiation even if the new object is immediately
    /// assigned to an array element. So it would make the resulting code a bit messier.</para>
    /// </remarks>
    public interface IObjectInitializerExpander : IDependency
    {
        void ExpandObjectInitializers(SyntaxTree syntaxTree);
    }
}
