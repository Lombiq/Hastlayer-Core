﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl
{
    public class TransformingContext
    {
        public SyntaxTree SyntaxTree { get; private set; }
        public Module Module { get; private set; }
        public List<InterfaceMethodDefinition> InterfaceMethods { get; private set; }
        public MethodCallChainTable MethodCallChainTable { get; private set; }


        public TransformingContext(SyntaxTree syntaxTree, Module module, MethodCallChainTable callChainTable)
        {
            SyntaxTree = syntaxTree;
            Module = module;
            InterfaceMethods = new List<InterfaceMethodDefinition>();
            MethodCallChainTable = callChainTable;
        }
    }
}
