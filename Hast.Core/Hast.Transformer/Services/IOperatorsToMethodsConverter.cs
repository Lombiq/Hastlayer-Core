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
    /// Converts operator overloads into standard methods.
    /// </summary>
    public interface IOperatorsToMethodsConverter : IDependency
    {
        void ConvertOperatorsToMethods(SyntaxTree syntaxTree);
    }
}
