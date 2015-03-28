using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Configuration
{
    public class HardwareGenerationConfiguration : IHardwareGenerationConfiguration
    {
        public int MaxDegreeOfParallelism { get; set; }
        public IDictionary<string, object> CustomConfiguration { get; set; }
        public IEnumerable<string> IncludedMembers { get; set; }

        private static HardwareGenerationConfiguration _default;
        public static HardwareGenerationConfiguration Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new HardwareGenerationConfiguration
                    {
                        MaxDegreeOfParallelism = 10
                    };
                }

                return _default;
            }
        }


        public HardwareGenerationConfiguration()
        {
            CustomConfiguration = new Dictionary<string, object>();
            IncludedMembers = Enumerable.Empty<string>();
        }
    }
}
