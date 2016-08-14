using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Extensibility.Pipeline
{
    public abstract class PipelineStepBase : IPipelineStep
    {
        public virtual double Priority { get; protected set; } = 0;
    }
}
