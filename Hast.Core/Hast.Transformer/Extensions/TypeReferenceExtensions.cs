using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mono.Cecil
{
    public static class TypeReferenceExtensions
    {
        public static bool IsSimpleMemory(this TypeReference typeReference) =>
            typeReference != null && typeReference.FullName == typeof(Hast.Transformer.SimpleMemory.SimpleMemory).FullName;
    }
}
