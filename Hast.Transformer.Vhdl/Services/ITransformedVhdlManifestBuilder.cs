using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Orchard;

namespace Hast.Transformer.Vhdl.Services
{
    public interface ITransformedVhdlManifestBuilder : IDependency
    {
        Task<ITransformedVhdlManifest> BuildManifest(ITransformationContext transformationContext);
    }
}
