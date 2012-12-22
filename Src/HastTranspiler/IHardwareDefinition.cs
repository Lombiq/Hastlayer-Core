using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastTranspiler
{
    public interface IHardwareDefinition
    {
        void WriteOut(Stream stream);
        void ReadIng(Stream stream);
    }
}
