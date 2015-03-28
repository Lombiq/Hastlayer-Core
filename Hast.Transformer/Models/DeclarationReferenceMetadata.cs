using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Models
{
    internal class DeclarationReferenceMetadata
    {
        public int ReferenceCount { get; set; }
        public bool IsReferenced { get { return ReferenceCount > 0; } }
    }
}
