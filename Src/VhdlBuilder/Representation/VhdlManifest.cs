using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
{
    public class VhdlManifest
    {
        public Module TopModule { get; set; }
        public List<Module> Modules { get; set; }

        public VhdlManifest()
        {
            Modules = new List<Module>();
        }
    }
}
