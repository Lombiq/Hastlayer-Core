﻿using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation;
using System.Linq;
using VhdlBuilder;
using HastTranspiler.Vhdl.SubTranspilers;
using System;

namespace HastTranspiler.Vhdl
{
    public class TranspilingEngine : ITranspilingEngine
    {
        private readonly TranspilingSettings _settings;

        public Func<string, TranspilingWorkflow> TranspilingWorkflowFactory { get; set; }


        public TranspilingEngine(TranspilingSettings settings)
        {
            _settings = settings;

            TranspilingWorkflowFactory = id => new TranspilingWorkflow(_settings, id);
        }


        public IHardwareDefinition Transpile(string id, SyntaxTree syntaxTree)
        {
            // This proxying is needed so this engine is not holding state. This is good for e.g. running multiple Transpile() calls in parallel.
            return TranspilingWorkflowFactory(id).Transpile(syntaxTree);
        }
    }
}
