using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.SampleAssembly;

namespace Hast.Samples.Consumer.SampleRunners
{
    internal class UnumCalculatorSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.AddHardwareEntryPointType<UnumCalculator>();

            configuration.TransformerConfiguration().AddLengthForMultipleArrays(
                UnumCalculator.EnvironmentFactory().EmptyBitMask.SegmentCount,
                new[]
                {
                    "System.UInt32[] Lombiq.Unum.BitMask::Segments()",
                    "System.Void Lombiq.Unum.BitMask::.ctor(System.UInt32[],System.UInt16).segments",
                    "System.Void Lombiq.Unum.BitMask::.ctor(System.UInt16,System.Boolean).array",
                    "System.Void Hast.Samples.SampleAssembly.UnumCalculator::CalculateSumOfPowersofTwo(Hast.Transformer.SimpleMemory.SimpleMemory).array",
                    "Lombiq.Unum.BitMask Lombiq.Unum.BitMask::op_Subtraction(Lombiq.Unum.BitMask, Lombiq.Unum.BitMask).array",
                    "Lombiq.Unum.BitMask Lombiq.Unum.BitMask::op_Addition(Lombiq.Unum.BitMask,Lombiq.Unum.BitMask).array",
                    "Lombiq.Unum.BitMask Lombiq.Unum.BitMask::op_Subtraction(Lombiq.Unum.BitMask,Lombiq.Unum.BitMask).array",
                    "Lombiq.Unum.BitMask Lombiq.Unum.BitMask::op_BitwiseOr(Lombiq.Unum.BitMask,Lombiq.Unum.BitMask).array",
                    "Lombiq.Unum.BitMask Lombiq.Unum.BitMask::op_ExclusiveOr(Lombiq.Unum.BitMask,Lombiq.Unum.BitMask).array",
                    "Lombiq.Unum.BitMask Lombiq.Unum.BitMask::op_BitwiseAnd(Lombiq.Unum.BitMask,Lombiq.Unum.BitMask).array",
                    //"System.UInt32[] Lombiq.Unum.Unum::FractionToUintArray()",
                    "Lombiq.Unum.BitMask Lombiq.Unum.BitMask::op_RightShift(Lombiq.Unum.BitMask,System.Int32).array",
                    "Lombiq.Unum.BitMask Lombiq.Unum.BitMask::op_LeftShift(Lombiq.Unum.BitMask,System.Int32).array",
                    "System.UInt32[] Lombiq.Unum.Unum::FractionToUintArray().array"
                });
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var unumCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new UnumCalculator());

            var result = unumCalculator.CalculateSumOfPowersofTwo(250);
        }
    }
}
