using System.IO;
using Hast.Common.Models;

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
