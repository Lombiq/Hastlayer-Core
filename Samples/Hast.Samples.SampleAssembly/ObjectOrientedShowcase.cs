﻿using System;
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
                new NumberContainer { Number = 24 },
                new NumberContainer(9)
            };

            // Array elements can be accessed and modified as usual.
            numberContainers1[1].IncreaseNumber(5);


            // Note that array dimensions need to be defined compile-time.
            var numberContainers2 = new NumberContainer[1];
            var numberContainer = new NumberContainer();
            numberContainer.Number = 5;
            if (!numberContainer.WasIncreased)
            {
                numberContainer.IncreaseNumber(5);
            }
            numberContainers2[0] = numberContainer;

            for (int i = 0; i < numberContainers1.Length; i++)
            {
                numberContainers1[i].IncreaseNumber(numberContainers2[0].Number);
            }

            // You can also pass arrays and other objects around to other methods.
            memory.WriteUInt32(Run_OutputUInt32Index, SumNumberCointainers(numberContainers1));
        }


        private uint SumNumberCointainers(NumberContainer[] numberContainers)
        {
            uint sum = 0;

            for (int i = 0; i < numberContainers.Length; i++)
            {
                sum += numberContainers[i].Number;
            }

            return sum;
        }
    }


    // Although this is a public class it could also be an inner class and/or a non-public one too.
    public class NumberContainer
    {
        // You can initialize properties C# 6-style too.
        public uint Number { get; set; } = 99;

        // Fields can be used too.
        public bool WasIncreased;


        public uint IncreaseNumber(uint increaseBy)
        {
            WasIncreased = true;
            return (Number += increaseBy);
        }


        // Constructors can be used, with or without parameters.
        public NumberContainer()
        {
        }

        public NumberContainer(uint number)
        {
            Number = number;
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