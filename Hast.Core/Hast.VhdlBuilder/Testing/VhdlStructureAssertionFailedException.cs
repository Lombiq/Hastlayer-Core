using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Testing
{
    public class VhdlStructureAssertionFailedException : Exception
    {
        // Putting the whole information into the Message is a bit ugly but the Shouldly test assertion package will
        // only display that, so all info should go there.
        public string Description { get; set; }
        public string CodeExcerpt { get; set; }

        public override string Message
        {
            get
            {
                return Description + Environment.NewLine + "Affected code:" + Environment.NewLine + CodeExcerpt;
            }
        }


        public VhdlStructureAssertionFailedException(string description, string codeExcerpt)
        {
            Description = description;
            CodeExcerpt = codeExcerpt;
        }
    }
}
