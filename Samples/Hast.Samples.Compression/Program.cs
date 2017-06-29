using Hast.Samples.Compression.Services;
using System;
using System.Threading.Tasks;

namespace Hast.Samples.Compression
{
    class Program
    {
        public const string InputFilePath = @"TestFiles\SmallTextFile.txt";
        public const string OutputFilePath = @"TestFiles\SmallTextFile-compressed.txt.lzma";


        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                using (var hastlayer = Xilinx.HastlayerFactory.Create())
                {
                    Console.WriteLine($"Compressing {InputFilePath}...");

                    await LzmaCompressionService.CompressFile(InputFilePath, OutputFilePath);

                    Console.WriteLine("Compression finished.");
                }
            }).Wait();

            Console.ReadKey();
        }
    }
}
