using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Layer
{
    public class HastlayerConfiguration : IHastlayerConfiguration
    {
        private static readonly HastlayerConfiguration _default = new HastlayerConfiguration();
        public static HastlayerConfiguration Default { get { return _default; } }

        public IEnumerable<Assembly> Extensions { get; set; }


        public HastlayerConfiguration()
        {
            Extensions = new List<Assembly>();
        }

        public HastlayerConfiguration(IHastlayerConfiguration previousConfiguration)
        {
            Extensions = previousConfiguration.Extensions;
        }
    }
}
