using Hast.Common.Models;
using Hast.Layer;
using Hast.TestInputs.Invalid;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Vhdl.Models;
using Lombiq.OrchardAppHost;
using NUnit.Framework;
using Shouldly;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
                    typeof(NotSupportedException));

                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidArrayUsingCases>(transformer, c => c.ArraySizeIsNotStatic(0)),
                    typeof(NotSupportedException));

                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidArrayUsingCases>(transformer, c => c.ArrayCopyToIsNotStaticCopy(0)),
                    typeof(NotSupportedException));

                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidArrayUsingCases>(transformer, c => c.UnsupportedImmutableArrayCreateRangeUsage()),
                    typeof(NotSupportedException));

                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidArrayUsingCases>(transformer, c => c.MultiDimensionalArray()),
                    typeof(NotSupportedException));

                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidArrayUsingCases>(transformer, c => c.NullAssignment()),
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

        [Test]
        public async Task InvalidLanguageConstructsArePrevented()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidLanguageConstructCases>(transformer, c => c.CustomValueTypeReferenceEquals()),
                    typeof(InvalidOperationException));

                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidLanguageConstructCases>(transformer, c => c.InvalidModelUsage()),
                    typeof(NotSupportedException));
            });
        }

        [Test]
        public async Task InvalidInvalidObjectUsingCasesArePrevented()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidObjectUsingCases>(transformer, c => c.ReferenceAssignment(0)),
                    typeof(NotSupportedException));

                await Should.ThrowAsync(() =>
                    TransformInvalidTestInputs<InvalidObjectUsingCases>(transformer, c => c.SelfReferencingType()),
                    typeof(NotSupportedException));
            });
        }


        private Task<VhdlHardwareDescription> TransformInvalidTestInputs<T>(
            ITransformer transformer,
            Expression<Action<T>> expression,
            bool useSimpleMemory = false)
        {
            return TransformAssembliesToVhdl(
                transformer,
                new[] { typeof(InvalidParallelCases).Assembly },
                configuration =>
                {
                    configuration.TransformerConfiguration().UseSimpleMemory = useSimpleMemory;
                    configuration.AddHardwareEntryPointMethod(expression);
                });
        }
    }
}
