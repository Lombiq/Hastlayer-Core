using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface ICompilerGeneratedClassesConverter : IDependency
    {
        void InlineCompilerGeneratedClasses(SyntaxTree syntaxTree);
    }
}
