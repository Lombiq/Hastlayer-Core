using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
