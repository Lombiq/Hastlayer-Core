using System.Collections.Generic;
using System.Reflection;

namespace Hast.Layer
{
    public interface IHastlayerConfiguration
    {
        /// <summary>
        /// Extensions that can provide implementations for Hastlayer services or hook into the hardware generation 
        /// pipeline.
        /// </summary>
        IEnumerable<Assembly> Extensions { get; }
    }
}
