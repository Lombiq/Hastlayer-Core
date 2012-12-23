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
        string Language { get; }
        void Save(Stream stream);
        void Load(Stream stream);
    }
}
