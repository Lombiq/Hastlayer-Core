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
    public interface IHardwareRepresentationComposer : IDependency
    {
        Task Compose(IHardwareDescription hardwareDescription);
    }
}
