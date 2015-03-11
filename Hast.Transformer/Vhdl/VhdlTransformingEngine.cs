using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation.Declaration;
using System.Linq;
using Hast.VhdlBuilder;
using Hast.Transformer.Vhdl.SubTransformers;
using System;
using Orchard.Environment.Extensions;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl
{
    [OrchardFeature("Hast.Transformer.Vhdl")]
    public class VhdlTransformingEngine : ITransformingEngine
    {
        private readonly TransformingSettings _settings;

        public Func<string, TransformingWorkflow> TransformingWorkflowFactory { get; set; }


        public VhdlTransformingEngine() : this(new TransformingSettings())
        {
        }

        public VhdlTransformingEngine(TransformingSettings settings)
        {
            _settings = settings;

            TransformingWorkflowFactory = id => new TransformingWorkflow(_settings, id);
        }


        public Task<IHardwareDefinition> Transform(string id, SyntaxTree syntaxTree)
        {
            // This proxying is needed so this engine is not holding state. This is good for e.g. running multiple Transform() calls in parallel.
            return TransformingWorkflowFactory(id).Transform(syntaxTree);
        }
    }
}
