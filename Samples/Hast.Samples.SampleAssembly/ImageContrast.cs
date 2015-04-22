using Hast.Transformer.SimpleMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly
{
    public class ImageContrast
    {
        private int ImageHeight { get; set; }
        private int ImageWidth { get; set; }
        private int PixelsCount { get { return ImageHeight * ImageWidth; } }
        private double ContrastValue { get; set; }


        public ImageContrast(int imageHeight, int imageWidth, double contrastValue = 50)
        {
            ImageHeight = imageHeight;
            ImageWidth = imageWidth;

            if (contrastValue > 100)
                contrastValue = 100;
            else if (contrastValue < -100)
                contrastValue = -100;

            ContrastValue = (100.0 + contrastValue) / 100.0;
        }


        public virtual void ProcessImage(SimpleMemory memory)
        {
            int pixel = 0;

            for (int i = 0; i < PixelsCount * 3; i++)
            {
                pixel = memory.ReadInt32((ulong)i);
                memory.WriteInt32((ulong)i, WorkUpPixel(pixel));
            }
        }


        private int WorkUpPixel(int pixel)
        {
            double correctedPixel = pixel / 255.0;
            correctedPixel -= 0.5;
            correctedPixel *= ContrastValue;
            correctedPixel += 0.5;
            correctedPixel *= 255;

            if (correctedPixel < 0) correctedPixel = 0;
            if (correctedPixel > 255) correctedPixel = 255;

            return (int)correctedPixel;
        }
    }
}
