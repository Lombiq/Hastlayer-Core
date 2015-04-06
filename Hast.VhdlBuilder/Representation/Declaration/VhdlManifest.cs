using System.Collections.Generic;

namespace Hast.VhdlBuilder.Representation.Declaration
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
