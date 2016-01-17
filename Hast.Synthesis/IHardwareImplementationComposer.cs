using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Models;
using Orchard;

namespace Hast.Synthesis
{
    public interface IHardwareImplementationComposer : IDependency
    {
        /// <summary>
        /// Composes the hardware implemenation for the give hardware representation.
        /// </summary>
        /// <param name="hardwareRepresentation">Represents the hardware that was generated from .NET assemblies.</param>
        /// <returns></returns>
        Task Compose(IHardwareRepresentation hardwareRepresentation);
    }
}
