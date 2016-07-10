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
            File.WriteAllText(Configuration.VhdlOutputFilePath, GenerateVhdl(hardwareRepresentation));
        }
        
        /// <summary>
        /// Generates a VHDL string from the given hardware representation.
        /// </summary>
        public static string GenerateVhdl(IHardwareRepresentation hardwareRepresentation)
        {
            // This will also nicely format the VHDL output just so we can take a look at it.
            return ((Hast.Transformer.Vhdl.Models.VhdlHardwareDescription)hardwareRepresentation.HardwareDescription)
                .Manifest
                .TopModule
                .ToVhdl(new VhdlGenerationOptions
                {
                    FormatCode = true,
                    NameShortener = VhdlGenerationOptions.SimpleNameShortener
                });
        }
    }
}
