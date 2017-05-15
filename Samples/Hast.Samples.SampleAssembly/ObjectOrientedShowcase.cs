using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SimpleMemory;

namespace Hast.Samples.SampleAssembly
{
    /// <summary>
    /// Object-oriented code can be written with Hastlayer as usual. This will also be directly mapped to hardware.
    /// </summary>
    public class ObjectOrientedShowcase
    {
        public const int Run_InputUInt32Index = 0;
        public const int Run_OutputUInt32Index = 0;


        public virtual void Run(SimpleMemory memory)
        {
            var inputNumber = memory.ReadUInt32(Run_InputUInt32Index);

            // Arrays can be initialized as usual, as well as objects.
            var numberContainers1 = new[]
            {
                new NumberContainer { Number = inputNumber },
                new NumberContainer { Number = inputNumber + 4 },
                new NumberContainer { Number = 24 }
            };

            // Array elements can be accessed and modified as usual.
            numberContainers1[1].IncreaseNumber(5);


            // Note that array dimensions need to be defined compile-time.
            var numberContainers2 = new NumberContainer[1];
            var numberContainer = new NumberContainer();
            numberContainer.Number = 5;
            numberContainer.IncreaseNumber(5);

            for (int i = 0; i < 3; i++)
            {
                numberContainers1[i].IncreaseNumber(numberContainer.Number);
            }

            uint sum = 0;
            for (int i = 0; i < 3; i++)
            {
                sum += numberContainers1[i].Number;
            }

            memory.WriteUInt32(Run_OutputUInt32Index, sum);
        }
    }


    // Although this is a public class it could also be an inner class and/or a non-public one too.
    public class NumberContainer
    {
        public uint Number { get; set; }


        public uint IncreaseNumber(uint increaseBy)
        {
            return (Number += increaseBy);
        }
    }



    public static class ObjectOrientedShowcaseExtensions
    {
        public static uint Run(this ObjectOrientedShowcase algorithm, uint input)
        {
            var memory = new SimpleMemory(1);
            memory.WriteUInt32(ObjectOrientedShowcase.Run_InputUInt32Index, input);
            algorithm.Run(memory);
            return memory.ReadUInt32(ObjectOrientedShowcase.Run_OutputUInt32Index);
        }
    }
}
