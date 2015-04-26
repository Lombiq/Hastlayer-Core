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
    /// <summary>
    /// Algorithm for changing the contrast of an image.
    /// </summary>
    public class ImageContrast
    {
        public const int ImageHeight_Index = 0;
        public const int ImageWidth_Index = 1;
        public const int ContrastValue_Index = 2;
        public const int ImageStart_Index = 3;


        /// <summary>
        /// Changes the contrast of an image.
        /// </summary>
        /// <param name="memory">The <see cref="SimpleMemory"/> object representing the accessible memory space.</param>
        public virtual void ChangeContrast(SimpleMemory memory)
        {
            int imageWidth = memory.ReadInt32(ImageWidth_Index);
            int imageHeight = memory.ReadInt32(ImageHeight_Index);
            double contrastValue = memory.ReadInt32(ContrastValue_Index);
            int pixel = 0;

            if (contrastValue > 100)
                contrastValue = 100;
            else if (contrastValue < -100)
                contrastValue = -100;

            contrastValue = (100.0 + contrastValue) / 100.0;

            for (int i = 0; i < imageHeight * imageWidth * 3; i++)
            {
                pixel = memory.ReadInt32(i + ImageStart_Index);
                memory.WriteInt32(i + ImageStart_Index, WorkUpPixel(pixel, contrastValue));
            }
        }


        /// <summary>
        /// Makes the required changes on the selected pixel.
        /// </summary>
        /// <param name="pixel">The current pixel value.</param>
        /// <param name="contrastValue">The contast difference value.</param>
        /// <returns>The pixel value after changing the contrast.</returns>
        private int WorkUpPixel(int pixel, double contrastValue)
        {
            double correctedPixel = pixel / 255.0;
            correctedPixel -= 0.5;
            correctedPixel *= contrastValue;
            correctedPixel += 0.5;
            correctedPixel *= 255;

            if (correctedPixel < 0) correctedPixel = 0;
            if (correctedPixel > 255) correctedPixel = 255;

            return (int)correctedPixel;
        }
    }


    public static class ImageContrastExtensions
    {
        public static Bitmap GetContrastImage(this ImageContrast imageContrast, Bitmap image, int contrast)
        {
            //Change image contrast

            var memory = CreateSimpleMemory(
                image,
                contrast,
                ImageContrast.ImageStart_Index,
                ImageContrast.ImageWidth_Index,
                ImageContrast.ImageHeight_Index,
                ImageContrast.ContrastValue_Index);
            imageContrast.ChangeContrast(memory);
            return CreateImage(memory, image, ImageContrast.ImageStart_Index);
        }


        /// <summary>
        /// Creates a <see cref="SimpleMemory"/> instance that stores the image.
        /// </summary>
        /// <param name="image">The image to process.</param>
        /// <param name="cloneImage">Is image cloning required?</param>
        /// <returns>The instance of the created <see cref="SimpleMemory"/>.</returns>
        private static SimpleMemory CreateSimpleMemory(Bitmap image, int contrastValue, int startIndex, int widthIndex, int heightIndex, int contrastIndex)
        {
            SimpleMemory memory = new SimpleMemory(image.Width * image.Height * 3 + 3);

            memory.WriteInt32(widthIndex, image.Width);
            memory.WriteInt32(heightIndex, image.Height);
            memory.WriteInt32(contrastIndex, contrastValue);

            for (int x = 0; x < image.Height; x++)
            {
                for (int y = 0; y < image.Width; y++)
                {
                    var pixel = image.GetPixel(y, x);

                    memory.WriteInt32((x * image.Width + y) * 3 + startIndex, pixel.R);
                    memory.WriteInt32((x * image.Width + y) * 3 + 1 + startIndex, pixel.G);
                    memory.WriteInt32((x * image.Width + y) * 3 + 2 + startIndex, pixel.B);
                }
            }

            return memory;
        }

        /// <summary>
        /// Creates an image from a <see cref="SimpleMemory"/> instance.
        /// </summary>
        /// <param name="memory">The <see cref="SimpleMemory"/> instance.</param>
        /// <param name="image">The original image.</param>
        /// <returns>Returns the processed image.</returns>
        private static Bitmap CreateImage(SimpleMemory memory, Bitmap image, int startIndex)
        {
            Bitmap newImage = new Bitmap(image);

            int r, g, b;

            for (int x = 0; x < newImage.Height; x++)
            {
                for (int y = 0; y < newImage.Width; y++)
                {
                    r = memory.ReadInt32((x * newImage.Width + y) * 3 + startIndex);
                    g = memory.ReadInt32((x * newImage.Width + y) * 3 + 1 + startIndex);
                    b = memory.ReadInt32((x * newImage.Width + y) * 3 + 2 + startIndex);

                    newImage.SetPixel(y, x, Color.FromArgb(r, g, b));
                }
            }

            return newImage;
        }
    }
}
