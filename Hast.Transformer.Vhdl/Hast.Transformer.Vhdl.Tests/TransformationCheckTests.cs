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
using System.Linq.Expressions;

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
                    TransformInvalidTestInputs<InvalidParallelCases>(transformer, c => c.InvalidExternalVariableAssignment(0)),
                    typeof(NotSupportedException));
            });
        }

        [Test]
        public async Task InvalidArrayUsageIsPrevented()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidArrayUsingCases>(transformer, c => c.InvalidArrayAssignment()),
                    typeof(InvalidOperationException));

                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidArrayUsingCases>(transformer, c => c.ArraySizeIsNotStatic(0)),
                    typeof(NotSupportedException));
            });
        }

        [Test]
        public async Task InvalidLanguageConstructsArePrevented()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidLanguageConstructCases>(transformer, c => c.BreakStatements()),
                    typeof(NotSupportedException));
            });
        }


        private Task<VhdlHardwareDescription> TransformInvalidTestInputs<T>(
            ITransformer transformer,
            Expression<Action<T>> expression)
        {
            return TransformAssembliesToVhdl(
                transformer,
                new[] { typeof(InvalidParallelCases).Assembly },
                configuration =>
                {
                    configuration.TransformerConfiguration().UseSimpleMemory = false;
                    configuration.AddPublicHardwareMethod<T>(expression);
                });
        }
    }
}
