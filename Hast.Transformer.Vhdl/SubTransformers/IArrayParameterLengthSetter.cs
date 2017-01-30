using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Service for statically setting the size of arrays which were passed as method parameters.
    /// </summary>
    public interface IArrayParameterLengthSetter : IDependency
    {
        void SetArrayParameterSizes(SyntaxTree syntaxTree);
    }
}
