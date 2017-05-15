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
    /// Converts constructors to normal method declarations so they're easier to process later.
    /// </summary>
    public interface IConstructorsToMethodsConverter : IDependency
    {
        void ConvertConstructorsToMethods(SyntaxTree syntaxTree);
    }
}
