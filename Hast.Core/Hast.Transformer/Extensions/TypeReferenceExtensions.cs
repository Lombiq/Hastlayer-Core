using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Abstractions.SimpleMemory;

namespace Mono.Cecil
{
    public static class TypeReferenceExtensions
    {
        public static bool IsSimpleMemory(this TypeReference typeReference) =>
            typeReference != null && typeReference.FullName == typeof(SimpleMemory).FullName;
    }
}
