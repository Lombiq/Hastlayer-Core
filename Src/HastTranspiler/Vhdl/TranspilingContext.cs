using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation.Declaration;

namespace HastTranspiler.Vhdl
{
    public class TranspilingContext
    {
        public SyntaxTree SyntaxTree { get; private set; }
        public Module Module { get; private set; }
        public List<InterfaceMethodDefinition> InterfaceMethods { get; private set; }
        public CallChainTable CallChainTable { get; private set; }


        public TranspilingContext(SyntaxTree syntaxTree, Module module, CallChainTable callChainTable)
        {
            SyntaxTree = syntaxTree;
            Module = module;
            InterfaceMethods = new List<InterfaceMethodDefinition>();
            CallChainTable = callChainTable;
        }
    }
}
