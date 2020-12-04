using Hast.Transformer.Models;
using ICSharpCode.Decompiler.TypeSystem;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    public interface IKnownTypeLookupTableFactory : IDependency
    {
        IKnownTypeLookupTable Create(ICompilation compilation);
    }

    public class KnownTypeLookupTableFactory : IKnownTypeLookupTableFactory
    {
        public IKnownTypeLookupTable Create(ICompilation compilation) => new KnownTypeLookupTable(compilation);

        private class KnownTypeLookupTable : IKnownTypeLookupTable
        {
            private readonly ICompilation _compilation;

            public KnownTypeLookupTable(ICompilation compilation)
            {
                _compilation = compilation;
            }

            public IType Lookup(KnownTypeCode typeCode) => _compilation.FindType(typeCode);
        }
    }
}
