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
        public const int AddToUnum_InputUInt32Index = 0;
        public const int AddToUnum_OutputUInt32Index = 0;

        public virtual void AddToUnum(SimpleMemory memory)
        {
            var environment = new UnumMetadata(4, 3);

            var input = memory.ReadUInt32(AddToUnum_InputUInt32Index);

            var a = new Unum(environment, input);

            // a.FractionSize() works, running the body of Fraction() here works as well, but calling the method yields
            // incorrect results:
            //var fractionMaskTest = new BitMask(new uint[] { 1 }, 33);
            //for (int i = 0; i < fractionMaskTest.Segments.Length; i++)
            //{
            //    memory.WriteUInt32(AddToUnum_OutputUInt32Index + i + offset, fractionMaskTest.Segments[i]);
            //}

            //offset += fractionMaskTest.Segments.Length;

            //var fractionMaskTestResult = fractionMaskTest << 3;
            //for (int i = 0; i < fractionMaskTestResult.Segments.Length; i++)
            //{
            //    memory.WriteUInt32(AddToUnum_OutputUInt32Index + i + offset, fractionMaskTestResult.Segments[i]);
            //}


            var offset = 1;


            // 7, 0
            //var fraction = a.Fraction();
            //for (int i = 0; i < fraction.Segments.Length; i++)
            //{
            //    memory.WriteUInt32(AddToUnum_OutputUInt32Index + i + offset, fraction.Segments[i]);
            //}

            //offset += fraction.Segments.Length;
            // Should be: 1792, 0
            var fractionMask = a.FractionMask();
            for (int i = 0; i < fractionMask.Segments.Length; i++)
            {
                memory.WriteUInt32(AddToUnum_OutputUInt32Index + i + offset, fractionMask.Segments[i]);
            }

            memory.WriteUInt32(0, (uint)a);

            //var b = new Unum(environment, (uint)5);
            //var result = a + b;

            //// 7, 0
            //for (int i = 0; i < result.UnumBits.Segments.Length; i++)
            //{
            //    memory.WriteUInt32(AddToUnum_OutputUInt32Index + i + 1, result.UnumBits.Segments[i]);
            //}

            //memory.WriteUInt32(AddToUnum_OutputUInt32Index, (uint)result);
        }
    }


    public static class UnumCalculatorExtensions
    {
        public static uint[] AddToUnum(this UnumCalculator unumCalculator, uint number)
        {
            var memory = new SimpleMemory(100);
            memory.WriteUInt32(UnumCalculator.AddToUnum_InputUInt32Index, number);
            unumCalculator.AddToUnum(memory);
            var resultArray = new uint[100];
            for (var i = 0; i < 100; i++)
            {
                resultArray[i] = memory.ReadUInt32(UnumCalculator.AddToUnum_OutputUInt32Index + i);
            }
            return resultArray;
        }
    }
}
