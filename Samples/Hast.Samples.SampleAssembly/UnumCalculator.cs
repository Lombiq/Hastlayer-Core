using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;
using Lombiq.Unum;

namespace Hast.Samples.SampleAssembly
{
    public class UnumCalculator
    {
        public const int CalculateSumOfPowersofTwo_InputUInt32Index = 0;
        public const int CalculateSumOfPowersofTwo_OutputUInt32Index = 0;

        public virtual void CalculateSumOfPowersofTwo(SimpleMemory memory)
        {
            var number = memory.ReadUInt32(CalculateSumOfPowersofTwo_InputUInt32Index);

            var environment = EnvironmentFactory();

            var a = new Unum(environment, 1);
            var b = new Unum(environment, 0);

            for (var i = 1; i <= number; i++)
            {
                b += a;
                a += a;
            }

            var resultArray = b.FractionToUintArray();
            for (var i = 0; i < 9; i++)
            {
                memory.WriteUInt32(CalculateSumOfPowersofTwo_OutputUInt32Index + i, resultArray[i]);
            }
        }

        // Needed so UnumCalculatorSampleRunner can retrieve BitMask.SegmentCount.
        public static UnumEnvironment EnvironmentFactory() => new UnumEnvironment(4, 8);
    }


    public static class UnumCalculatorExtensions
    {
        public static uint[] CalculateSumOfPowersofTwo(this UnumCalculator unumCalculator, uint number)
        {
            var memory = new SimpleMemory(9);
            memory.WriteUInt32(UnumCalculator.CalculateSumOfPowersofTwo_InputUInt32Index, number);
            unumCalculator.CalculateSumOfPowersofTwo(memory);
            var resultArray = new uint[9];
            for (var i = 0; i < 9; i++)
            {
                resultArray[i] = memory.ReadUInt32(UnumCalculator.CalculateSumOfPowersofTwo_OutputUInt32Index + i);
            }
            return resultArray;
        }
    }
}
