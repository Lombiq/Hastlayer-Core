using Hast.Common.Interfaces;
using Hast.Layer;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    public interface IReadonlyToConstConverter : IDependency
    {
        void ConvertReadonlyPrimitives(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration);
    }
}
