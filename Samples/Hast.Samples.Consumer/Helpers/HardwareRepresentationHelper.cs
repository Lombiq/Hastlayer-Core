using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Models;
using Hast.VhdlBuilder.Representation;

namespace Hast.Samples.Consumer.Helpers
{
    internal static class HardwareRepresentationHelper
    {
        public static void WriteVhdlToFile(IHardwareRepresentation hardwareRepresentation)
        {
            File.WriteAllText(
                Configuration.VhdlOutputFilePath,
                ((Transformer.Vhdl.Models.VhdlHardwareDescription)hardwareRepresentation.HardwareDescription).VhdlSource);
        }
    }
}
