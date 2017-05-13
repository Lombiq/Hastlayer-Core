using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Hast.Algorithms;

namespace Hast.Tests.MWC64XTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: " + System.AppDomain.CurrentDomain.FriendlyName + " <num_random_samples> <output_file_path>");
                return;
            }
            BinaryWriter bw = new BinaryWriter(new FileStream(args[1], FileMode.Create));
            MWC64X prng = new MWC64X();
            int numSamples = int.Parse(args[0]);
            for (int i = 0; i < numSamples; i++)
            {
                bw.Write(prng.GetNextRandom());
            }
        }
    }
}

