using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Models;

namespace Hast.Layer.Extensibility.Events
{
    public class TransformedEventArgs : EventArgs
    {
        private IHardwareDescription _hardwareDescription;
        public IHardwareDescription HardwareDescription
        {
            get { return _hardwareDescription; }
        }


        public TransformedEventArgs(IHardwareDescription hardwareDescription)
        {
            _hardwareDescription = hardwareDescription;
        }
    }
}
