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
    /// Substitutes variables, fields, etc. with constants if they can only ever have a compile-time defined value.
    /// </summary>
    public interface IConstantValuesSubstituter : IDependency
    {
        void SubstituteConstantValues(SyntaxTree syntaxTree);
    }
}
