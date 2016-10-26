using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Models
{
    public class ParameterArrayLength
    {
        public int Length { get; private set; }


        public ParameterArrayLength(int length)
        {
            Length = length;
        }
    }
}
