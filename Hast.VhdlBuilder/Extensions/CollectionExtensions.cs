using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds the given VHDL data object to the collection if one with the same name doesn't exist in the collection
        /// already.
        /// </summary>
        public static void AddIfNew<T>(this ICollection<T> collection, T item)
            where T : DataObjectBase
        {
            if (collection.Any(element => element.Name == item.Name)) return;
            collection.Add(item);
        }
    }
}
