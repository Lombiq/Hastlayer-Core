using Hast.Common.Interfaces;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.TypeSystem;

namespace Hast.Transformer.Services
{
    public interface IKnownTypeLookupTableFactory : IDependency
    {
        IKnownTypeLookupTable Create(ICompilation compilation);
    }
}
