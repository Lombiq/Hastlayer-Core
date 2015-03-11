using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard;

namespace Hast.Communication
{
    /// <summary>
    /// Generates proxies for objects whose logic is implemented as hardware to redirect method calls to the hardware implementation.
    /// </summary>
    public interface IProxyGenerator : ISingletonDependency
    {
        T CreateCommunicationProxy<T>(T target) where T : class;
    }
}
