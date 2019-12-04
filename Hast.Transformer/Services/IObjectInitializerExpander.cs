using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Converts inline object initializers into one-by-one property assignments so these can be transformed in a
    /// simpler way.
    /// </summary>
    /// <example>
    /// var x = new MyClass { Property1 = value1, Property2 = value2 };
    /// 
    /// will be converted to:
    /// 
    /// var x = new MyClass();
    /// x.Property1 = value1;
    /// x.Property2 = value2;
    /// </example>
    public interface IObjectInitializerExpander : IDependency
    {
        void ExpandObjectInitializers(SyntaxTree syntaxTree);
    }
}
