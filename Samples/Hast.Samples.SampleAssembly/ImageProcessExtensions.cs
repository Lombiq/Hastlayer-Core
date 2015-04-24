using Hast.Transformer.SimpleMemory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly
{
    public static class ImageProcessExtensions
    {
        public static Bitmap GetContrastImage(this ImageContrast imageContrast, Bitmap image, double contrast)
        {
            var memory = CreateSimpleMemory(image);
            imageContrast = new ImageContrast(image.Height, image.Width, contrast);
            imageContrast.ProcessImage(memory);
            return CreateImage(memory, image);
        }


        private static SimpleMemory CreateSimpleMemory(Bitmap image)
        {
            var memory = new SimpleMemory((ulong)(image.Width * image.Height * 3));

            for (int x = 0; x < image.Height; x++)
            {
                for (int y = 0; y < image.Width; y++)
                {
                    var pixel = image.GetPixel(y, x);

                    memory.WriteInt32((ulong)((x * image.Width + y) * 3), pixel.R);
                    memory.WriteInt32((ulong)((x * image.Width + y) * 3 + 1), pixel.G);
                    memory.WriteInt32((ulong)((x * image.Width + y) * 3 + 2), pixel.B);
                }
            }

            return memory;
        }

        private static Bitmap CreateImage(SimpleMemory memory, Bitmap image)
        {
            int r, g, b;

            for (int x = 0; x < image.Height; x++)
            {
                for (int y = 0; y < image.Width; y++)
                {
                    r = memory.ReadInt32((ulong)((x * image.Width + y) * 3));
                    g = memory.ReadInt32((ulong)((x * image.Width + y) * 3 + 1));
                    b = memory.ReadInt32((ulong)((x * image.Width + y) * 3 + 2));

                    image.SetPixel(y, x, Color.FromArgb(r, g, b));
                }
            }

            return image;
        }
    }
}
