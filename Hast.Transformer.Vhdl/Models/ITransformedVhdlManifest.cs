using Hast.Layer;
using Hast.VhdlBuilder.Representation.Declaration;
using System.Collections.Generic;

namespace Hast.Transformer.Vhdl.Models
{
    public interface ITransformedVhdlManifest
    {
        VhdlManifest Manifest { get; }
        MemberIdTable MemberIdTable { get; }
        IEnumerable<ITransformationWarning> Warnings { get; }

        /// <summary>
        /// Gets the Xilinx XDC file, only for Xilinx devices.
        /// </summary>
        XdcFile XdcFile { get; }
    }
}
