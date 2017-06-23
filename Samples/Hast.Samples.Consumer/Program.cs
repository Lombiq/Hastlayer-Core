using Lombiq.Unum;
using System;
using System.Linq;
using System.Numerics;

namespace Hast.Samples.Consumer
{
    // In this simple console application we generate hardware from some sample algorithms.

    // Configure the whole sample project here:
    internal static class Configuration
    {
        /// <summary>
        /// Specify a path here where the VHDL file describing the hardware to be generated will be saved. If the path
        /// is relative (like the default) then the file will be saved along this project's executable in the bin output
        /// directory. If an empty string or null is specified then no file will be generated.
        /// </summary>
        public static string VhdlOutputFilePath = @"Hast_IP.vhd";

        /// <summary>
        /// Which sample algorithm to transform and run? Choose one.
        /// </summary>
        public static Sample SampleToRun = Sample.UnumCalculator;
    }


    class Program
    {
        static void Main(string[] args)
        {
            var environment = new UnumMetadata(3, 4);
            Console.WriteLine($"Bits required for a 3;4 environment: {environment.Size}.");

            environment = new UnumMetadata(4, 8);
            Console.WriteLine($"Bits required for a 4;8 environment: {environment.Size}.");

            var unum = new Unum(environment, (uint)1);
            var sum = new Unum(environment, (uint)0);
            BigInteger representation;
            var resultBytes = new byte[unum.UnumBits.SegmentCount * 4];
            var interestingResults = new int[] { 64, 128, 200, 255 };

            for (int i = 1; i < 10; i++)
            {
                sum += unum;
                unum += unum;

                if (true)//(interestingResults.Contains(i))
                {
                    Buffer.BlockCopy(sum.UnumBits.Segments, 0, resultBytes, 0, resultBytes.Length);
                    representation = new BigInteger(resultBytes);

                    Console.WriteLine($"Sum of the first {i} powers of 2: {representation.ToString()}.");
                }
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }
}
