using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.TestInputs.Invalid;
using Hast.Transformer.Vhdl.Models;
using Lombiq.OrchardAppHost;
using NUnit.Framework;
using Shouldly;

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

        [Test]
        public async Task InvalidHardwareEntryPointsArePrevented()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidHardwareEntryPoint>(transformer, c => c.EntryPointMethod()),
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
                    configuration.AddHardwareEntryPointMethod(expression);
                });
        }
    }
}
