using Hast.Samples.Compression.Services;
using System;

namespace Hast.Samples.Compression
{
    class Program
    {
        public const string InputFilePath = @"TestFiles\LargeTextFile.txt";
        public const string OutputFilePath = @"TestFiles\LargeTextFile-compressed.txt.lzma";


        static void Main(string[] args)
        {
            //Task.Run(async () =>
            //{
                using (var hastlayer = Xilinx.HastlayerFactory.Create())
                {
                    Console.WriteLine($"Compressing {InputFilePath}...");

                    LzmaCompressionService.CompressFile(InputFilePath, OutputFilePath);

                    Console.WriteLine("Compression finished.");
                }
            //}).Wait();

            Console.ReadKey();
        }
    }
}
