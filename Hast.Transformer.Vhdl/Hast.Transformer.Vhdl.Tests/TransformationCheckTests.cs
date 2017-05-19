using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using NUnit.Framework;
using Lombiq.OrchardAppHost;
using Hast.Transformer.Vhdl.Models;
using Shouldly;
using Hast.TestInputs.Invalid;

namespace Hast.Transformer.Vhdl.Tests
{
    [TestFixture]
    public class TransformationCheckTests : VhdlTransformingTestFixtureBase
    {
        [Test]
        public async Task InvalidExternalVariableAssignmentIsPrevented()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs(transformer, "Hast.TestInputs.Invalid.InvalidParallelCases.InvalidExternalVariableAssignment"),
                    typeof(NotSupportedException));
            });
        }

        [Test]
        public async Task InvalidArrayUsageIsPrevented()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs(transformer, "Hast.TestInputs.Invalid.InvalidArrayUsingCases.InvalidArrayAssignment"),
                    typeof(InvalidOperationException));

                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs(transformer, "Hast.TestInputs.Invalid.InvalidArrayUsingCases.ArraySizeIsNotStatic"),
                    typeof(NotSupportedException));
            });
        }

        [Test]
        public async Task InvalidLanguageConstructsArePrevented()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs(transformer, "Hast.TestInputs.Invalid.InvalidLanguageConstructCases.BreakStatements"),
                    typeof(NotSupportedException));
            });
        }


        private Task<VhdlHardwareDescription> TransformInvalidTestInputs(
            ITransformer transformer, 
            string memberNamePrefixToInclude)
        {
            return TransformAssembliesToVhdl(
                transformer,
                new[] { typeof(InvalidParallelCases).Assembly },
                configuration =>
                {
                    configuration.TransformerConfiguration().UseSimpleMemory = false;
                    configuration.PublicHardwareMemberNamePrefixes.Add(memberNamePrefixToInclude);
                });
        }
    }
}
