using Hast.Transformer.SimpleMemory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly
{
    public class ImageContrast
    {
        private int _imageHeight;
        private int _imageWidth;
        private double _contrastValue;


        public ImageContrast(int imageHeight, int imageWidth, double contrastValue = 50)
        {
            _imageHeight = imageHeight;
            _imageWidth = imageWidth;

            if (contrastValue > 100)
                contrastValue = 100;
            else if (contrastValue < -100)
                contrastValue = -100;

            _contrastValue = (100.0 + contrastValue) / 100.0;
        }


        public virtual void ProcessImage(SimpleMemory memory)
        {
            int pixel = 0;

            for (int i = 0; i < _imageHeight * _imageWidth * 3; i++)
            {
                pixel = memory.ReadInt32((ulong)i);
                memory.WriteInt32((ulong)i, WorkUpPixel(pixel));
            }
        }


        private int WorkUpPixel(int pixel)
        {
            double correctedPixel = pixel / 255.0;
            correctedPixel -= 0.5;
            correctedPixel *= _contrastValue;
            correctedPixel += 0.5;
            correctedPixel *= 255;

            if (correctedPixel < 0) correctedPixel = 0;
            if (correctedPixel > 255) correctedPixel = 255;

            return (int)correctedPixel;
        }
    }

    public static class ImageContrastExtensions
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
