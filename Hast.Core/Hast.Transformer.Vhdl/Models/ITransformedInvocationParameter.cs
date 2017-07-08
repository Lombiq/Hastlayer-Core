using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public interface ITransformedInvocationParameter
    {
        IVhdlElement Reference { get; }
        DataType DataType { get; }
    }


    internal class TransformedInvocationParameter : ITransformedInvocationParameter
    {
        public IVhdlElement Reference { get; set; }
        public DataType DataType { get; set; }
    }
}
