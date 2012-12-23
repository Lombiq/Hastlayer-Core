using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhdlBuilder.Representation.Declaration;

namespace HastTranspiler.Vhdl
{
    public class TranspilingContext
    {
        public Module Module { get; private set; }
        public List<InterfaceMethodDefinition> InterfaceMethods { get; private set; }

        public TranspilingContext() : this(new Module())
        {
        }

        public TranspilingContext(Module module)
        {
            Module = module;
            InterfaceMethods = new List<InterfaceMethodDefinition>();
        }
    }
}
