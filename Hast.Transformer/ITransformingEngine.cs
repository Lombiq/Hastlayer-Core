using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer
{
    /// <summary>
    /// Described the concrete engine that does the .NET to hardware definition transformation. Implementation could include ones generating
    /// e.g. VHDL or Verilog code. 
    /// </summary>
    public interface ITransformingEngine : IDependency
    {
        Task<IHardwareDefinition> Transform(string id, SyntaxTree syntaxTree);
    }
}
