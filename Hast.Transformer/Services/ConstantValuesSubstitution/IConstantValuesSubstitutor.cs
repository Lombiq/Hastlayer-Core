using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    /// <summary>
    /// Substitutes variables, fields, etc. with constants if they can only ever have a compile-time defined value.
    /// </summary>
    public interface IConstantValuesSubstitutor : IDependency
    {
        IArraySizeHolder SubstituteConstantValues(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
