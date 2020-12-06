using System.Collections.Generic;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public interface IBlockElement : IVhdlElement
    {
        List<IVhdlElement> Body { get; }
    }

    public static class BlockElementExtensions
    {
        public static void Add(this IBlockElement blockElement, IVhdlElement element) => blockElement.Body.Add(element);
    }
}
