using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Configuration
{
    public class TransformationWorkerConfiguration : ITransformationWorkerConfiguration
    {
        public string StorageConnectionString { get; set; }
    }
}
