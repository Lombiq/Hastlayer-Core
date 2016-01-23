using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Models;
using Hast.Synthesis.Models;
using Orchard;

namespace Hast.Synthesis
{
    public interface IHardwareImplementationComposer : IDependency
    {
        /// <summary>
        /// Composes the hardware implemenation for the give hardware representation.
        /// </summary>
        /// <param name="hardwareDescription">Represents the hardware that was generated from .NET assemblies.</param>
        /// <returns>The handle to the synthesized hardware implementation.</returns>
        Task<IHardwareImplementation> Compose(IHardwareDescription hardwareDescription);
    }
}
