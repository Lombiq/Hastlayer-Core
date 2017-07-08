﻿using System.Drawing;
using Hast.Transformer.Abstractions.SimpleMemory;

namespace Hast.Samples.SampleAssembly
{
    /// <summary>
    /// Algorithm for changing the contrast of an image.
    /// </summary>
    public class ImageContrastModifier
    {
        private const int Multiplier = 1000;

        public const int ChangeContrast_ImageHeightIndex = 0;
        public const int ChangeContrast_ImageWidthIndex = 1;
        public const int ChangeContrast_ContrastValueIndex = 2;
        public const int ChangeContrast_ImageStartIndex = 3;


        /// <summary>
        /// Changes the contrast of an image.
        /// </summary>
        /// <param name="memory">The <see cref="SimpleMemory"/> object representing the accessible memory space.</param>
        public virtual void ChangeContrast(SimpleMemory memory)
        {
            ushort imageWidth = (ushort)memory.ReadUInt32(ChangeContrast_ImageWidthIndex);
            ushort imageHeight = (ushort)memory.ReadUInt32(ChangeContrast_ImageHeightIndex);
            int contrastValue = memory.ReadInt32(ChangeContrast_ContrastValueIndex);
            ushort pixel = 0;

            if (contrastValue > 100)
                contrastValue = 100;
            else if (contrastValue < -100)
                contrastValue = -100;

            contrastValue = (100 + contrastValue * Multiplier) / 100;

            for (int i = 0; i < imageHeight * imageWidth * 3; i++)
            {
                pixel = (ushort)memory.ReadUInt32(i + ChangeContrast_ImageStartIndex);
                memory.WriteUInt32(i + ChangeContrast_ImageStartIndex, ChangePixelValue(pixel, contrastValue));
            }
        }


        /// <summary>
        /// Makes the required changes on the selected pixel.
        /// </summary>
        /// <param name="pixel">The current pixel value.</param>
        /// <param name="contrastValue">The contrast difference value.</param>
        /// <returns>The pixel value after changing the contrast.</returns>
        private ushort ChangePixelValue(ushort pixel, int contrastValue)
        {
            int correctedPixel = pixel * Multiplier / 255;
            correctedPixel -= (int)(0.5 * Multiplier); 
            correctedPixel *= contrastValue;
            correctedPixel /= Multiplier;
            correctedPixel += (int)(0.5 * Multiplier);
            correctedPixel *= 255; 
            correctedPixel /= Multiplier;

            if (correctedPixel < 0) correctedPixel = 0;
            if (correctedPixel > 255) correctedPixel = 255;

            return (ushort)correctedPixel;
        }
    }


    public static class ImageContrastModifierExtensions
    {
        /// <summary>
        /// Changes the contrast of an image.
        /// </summary>
        /// <param name="image">The image that we modify.</param>
        /// <param name="contrast">The value of the intensity to calculate the new pixel values.</param>
        /// <returns>Returns an image with changed contrast values.</returns>
        public static Bitmap ChangeImageContrast(this ImageContrastModifier imageContrast, Bitmap image, int contrast)
        {
            var memory = CreateSimpleMemory(
                image,
                contrast);
            imageContrast.ChangeContrast(memory);
            return CreateImage(memory, image);
        }


        /// <summary>
        /// Creates a <see cref="SimpleMemory"/> instance that stores the image.
        /// </summary>
        /// <param name="image">The image to process.</param>
        /// <param name="contrastValue">The contrast difference value.</param>
        /// <returns>The instance of the created <see cref="SimpleMemory"/>.</returns>
        private static SimpleMemory CreateSimpleMemory(Bitmap image, int contrastValue)
        {
            SimpleMemory memory = new SimpleMemory(image.Width * image.Height * 3 + 3);

            memory.WriteUInt32(ImageContrastModifier.ChangeContrast_ImageWidthIndex, (uint)image.Width);
            memory.WriteUInt32(ImageContrastModifier.ChangeContrast_ImageHeightIndex, (uint)image.Height);
            memory.WriteInt32(ImageContrastModifier.ChangeContrast_ContrastValueIndex, contrastValue);

            for (int x = 0; x < image.Height; x++)
            {
                for (int y = 0; y < image.Width; y++)
                {
                    var pixel = image.GetPixel(y, x);

                    memory.WriteUInt32((x * image.Width + y) * 3 + ImageContrastModifier.ChangeContrast_ImageStartIndex, pixel.R);
                    memory.WriteUInt32((x * image.Width + y) * 3 + 1 + ImageContrastModifier.ChangeContrast_ImageStartIndex, pixel.G);
                    memory.WriteUInt32((x * image.Width + y) * 3 + 2 + ImageContrastModifier.ChangeContrast_ImageStartIndex, pixel.B);
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
        private static Bitmap CreateImage(SimpleMemory memory, Bitmap image)
        {
            Bitmap newImage = new Bitmap(image);

            int r, g, b;

            for (int x = 0; x < newImage.Height; x++)
            {
                for (int y = 0; y < newImage.Width; y++)
                {
                    r = memory.ReadInt32((x * newImage.Width + y) * 3 + ImageContrastModifier.ChangeContrast_ImageStartIndex);
                    g = memory.ReadInt32((x * newImage.Width + y) * 3 + 1 + ImageContrastModifier.ChangeContrast_ImageStartIndex);
                    b = memory.ReadInt32((x * newImage.Width + y) * 3 + 2 + ImageContrastModifier.ChangeContrast_ImageStartIndex);

                    newImage.SetPixel(y, x, Color.FromArgb(r, g, b));
                }
            }

            return newImage;
        }
    }
}
