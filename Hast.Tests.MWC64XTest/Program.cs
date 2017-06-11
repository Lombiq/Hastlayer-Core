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
            Main2();
            return;
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

        static void Main2()
        {
            MWC64X prng = new MWC64X();
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(String.Format("GetNextRandom: {0}, GetNextRandom1: {1}", prng.GetNextRandom(), prng.GetNextRandom1()));
            }
            Console.Read();
        }
    }
}

