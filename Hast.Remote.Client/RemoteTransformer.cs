using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Extensions;

namespace Hast.Remote.Client
{
    public class RemoteTransformer : ITransformer
    {
        public Task<IHardwareDescription> Transform(IEnumerable<string> assemblyPaths, IHardwareGenerationConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}
