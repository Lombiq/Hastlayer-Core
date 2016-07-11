using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Models;
using Orchard;
using Hast.Common.Configuration;

namespace Hast.Communication
{
    /// <summary>
    /// Generates proxies for objects whose logic is implemented as hardware to redirect member calls to the hardware 
    /// implementation.
    /// </summary>
    public interface IProxyGenerator : ISingletonDependency
    {
        T CreateCommunicationProxy<T>(IHardwareRepresentation hardwareRepresentation, T target, IProxyGenerationConfiguration configuration) where T : class;
    }
}
