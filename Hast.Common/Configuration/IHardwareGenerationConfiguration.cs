using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Configuration
{
    public interface IHardwareGenerationConfiguration
    {
        int MaxDegreeOfParallelism { get; }

        /// <summary>
        /// Contains settings for non-default configuration options (like ones required by specific transformer
        /// implementations).
        /// </summary>
        IDictionary<string, object> CustomConfiguration { get; }
    }


    public class HardwareGenerationConfiguration : IHardwareGenerationConfiguration
    {
        public int MaxDegreeOfParallelism { get; set; }
        public IDictionary<string, object> CustomConfiguration { get; set; }
    }
}
