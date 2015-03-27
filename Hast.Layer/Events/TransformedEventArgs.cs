using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;

namespace Hast.Layer
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
