﻿using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation.Declaration;
using System.Linq;
using Hast.VhdlBuilder;
using Hast.Transformer.Vhdl.SubTransformers;
using System;
using Orchard.Environment.Extensions;
using System.Threading.Tasks;
using Hast.Common.Configuration;

namespace Hast.Transformer.Vhdl
{
    [OrchardFeature("Hast.Transformer.Vhdl")]
    public class VhdlTransformingEngine : ITransformingEngine
    {
        public Task<IHardwareDefinition> Transform(string id, SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration)
        {
            // This proxying is needed so this engine is not holding state. This is good for e.g. running multiple Transform() calls in parallel.
            return new TransformingWorkflow(configuration, id).Transform(syntaxTree);
        }
    }
}
