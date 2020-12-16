using ICSharpCode.Decompiler.TypeSystem;

namespace Hast.Transformer.Models
{
    public interface IKnownTypeLookupTable
    {
        /// <summary>
        /// Retrieves the <see cref="IType"/> of a known type.
        /// </summary>
        /// <param name="typeCode"></param>
        /// <returns></returns>
        IType Lookup(KnownTypeCode typeCode);
    }
}
