﻿using Hast.Transformer.SimpleMemory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly
{
    /// <summary>
    /// Algorithm for running convolution image processing on images.
    /// </summary>
    public class ImageFilter
    {
        public const int FilterImage_ImageHeightIndex = 0;
        public const int FilterImage_ImageWidthIndex = 1;
        public const int FilterImage_TopLeftIndex = 2;
        public const int FilterImage_TopMiddleIndex = 3;
        public const int FilterImage_TopRightIndex = 4;
        public const int FilterImage_MiddleLeftIndex = 5;
        public const int FilterImage_PixelIndex = 6;
        public const int FilterImage_MiddleRightIndex = 7;
        public const int FilterImage_BottomLeftIndex = 8;
        public const int FilterImage_BottomMiddleIndex = 9;
        public const int FilterImage_BottomRightIndex = 10;
        public const int FilterImage_FactorIndex = 11;
        public const int FilterImage_OffsetIndex = 12;
        public const int FilterImage_ImageStartIndex = 13;


        /// <summary>
        /// Makes the changes according to the matrix on the image.
        /// </summary>
        /// <param name="memory">The <see cref="SimpleMemory"/> object representing the accessible memory space.</param>
        public virtual void FilterImage(SimpleMemory memory)
        {
            ushort imageWidth = (ushort)memory.ReadUInt32(FilterImage_ImageWidthIndex);
            ushort imageHeight = (ushort)memory.ReadUInt32(FilterImage_ImageHeightIndex);

            int factor = memory.ReadInt32(FilterImage_FactorIndex);
            int offset = memory.ReadInt32(FilterImage_OffsetIndex);
            int topLeftValue = memory.ReadInt32(FilterImage_TopLeftIndex);
            int topMiddleValue = memory.ReadInt32(FilterImage_TopMiddleIndex);
            int topRightValue = memory.ReadInt32(FilterImage_TopRightIndex);
            int middleLeftValue = memory.ReadInt32(FilterImage_MiddleLeftIndex);
            int pixelValue = memory.ReadInt32(FilterImage_PixelIndex);
            int middleRightValue = memory.ReadInt32(FilterImage_MiddleRightIndex);
            int bottomLeftValue = memory.ReadInt32(FilterImage_BottomLeftIndex);
            int bottomMiddleValue = memory.ReadInt32(FilterImage_BottomMiddleIndex);
            int bottomRightValue = memory.ReadInt32(FilterImage_BottomRightIndex);

            ushort topLeft = 0;
            ushort topMiddle = 0;
            ushort topRight = 0;
            ushort middleLeft = 0;
            ushort pixel = 0;
            ushort middleRight = 0;
            ushort bottomLeft = 0;
            ushort bottomMiddle = 0;
            ushort bottomRight = 0;

            int pixelCountHelper = imageHeight * imageWidth * 3;
            ushort imageWidthHelper = (ushort)(imageWidth * 3);

            for (ushort x = 1; x < imageHeight - 1; x++)
            {
                for (ushort y = 3; y < imageWidthHelper - 3; y++)
                {
                    topLeft = (ushort)memory.ReadUInt32(x * imageWidthHelper + y + pixelCountHelper - imageWidthHelper - 3 + FilterImage_ImageStartIndex);
                    topMiddle = (ushort)memory.ReadUInt32(x * imageWidthHelper + y + pixelCountHelper - imageWidthHelper + FilterImage_ImageStartIndex);
                    topRight = (ushort)memory.ReadUInt32(x * imageWidthHelper + y + pixelCountHelper - imageWidthHelper + 3 + FilterImage_ImageStartIndex);
                    middleLeft = (ushort)memory.ReadUInt32(x * imageWidthHelper + y + pixelCountHelper - 3 + FilterImage_ImageStartIndex);
                    pixel = (ushort)memory.ReadUInt32(x * imageWidthHelper + y + pixelCountHelper + FilterImage_ImageStartIndex);
                    middleRight = (ushort)memory.ReadUInt32(x * imageWidthHelper + y + pixelCountHelper + 3 + FilterImage_ImageStartIndex);
                    bottomLeft = (ushort)memory.ReadUInt32(x * imageWidthHelper + y + pixelCountHelper + imageWidthHelper - 3 + FilterImage_ImageStartIndex);
                    bottomMiddle = (ushort)memory.ReadUInt32(x * imageWidthHelper + y + pixelCountHelper + imageWidthHelper + FilterImage_ImageStartIndex);
                    bottomRight = (ushort)memory.ReadUInt32(x * imageWidthHelper + y + pixelCountHelper + imageWidthHelper + 3 + FilterImage_ImageStartIndex);

                    memory.WriteUInt32(x * imageWidthHelper + y + FilterImage_ImageStartIndex, CalculatePixelValue(
                        topLeft, topMiddle, topRight,
                        middleLeft, pixel, middleRight,
                        bottomLeft, bottomMiddle, bottomRight,
                        topLeftValue, topMiddleValue, topRightValue,
                        middleLeftValue, pixelValue, middleRightValue,
                        bottomLeftValue, bottomMiddleValue, bottomRightValue,
                        factor, offset));
                }
            }
        }


        /// <summary>
        /// Makes the required changes on the selected pixel.
        /// </summary>
        /// <param name="topLeft">Top left value.</param>
        /// <param name="topMiddle">Top middle value.</param>
        /// <param name="topRight">Top right value.</param>
        /// <param name="middleLeft">Middle left value.</param>
        /// <param name="pixel">The current pixel value.</param>
        /// <param name="middleRight">Middle right value.</param>
        /// <param name="bottomLeft">Bottom left value.</param>
        /// <param name="bottomMiddle">Bottom middle value.</param>
        /// <param name="bottomRight">Bottom right value.</param>
        /// <param name="topLeftValue">Top left value in matrix.</param>
        /// <param name="topMiddleValue">Top middle value in matrix.</param>
        /// <param name="topRightValue">Top right value in matrix.</param>
        /// <param name="middleLeftValue">Middle left value in matrix.</param>
        /// <param name="pixelValue">The current pixel value in matrix.</param>
        /// <param name="middleRightValue">Middle right value in matrix.</param>
        /// <param name="bottomLeftValue">Bottom left value in matrix.</param>
        /// <param name="bottomMiddleValue">Bottom middle value in matrix.</param>
        /// <param name="bottomRightValue">Bottom right value in matrix.</param>
        /// <param name="factor">The value to devide the summed matrix values with.</param>
        /// <param name="offset">Offset value added to the result.</param>
        /// <returns>Returns the value of the filtered pixel in matrix.</returns>
        private ushort CalculatePixelValue(
            ushort topLeft, ushort topMiddle, ushort topRight,
            ushort middleLeft, ushort pixel, ushort middleRight,
            ushort bottomLeft, ushort bottomMiddle, ushort bottomRight,
            int topLeftValue, int topMiddleValue, int topRightValue,
            int middleLeftValue, int pixelValue, int middleRightValue,
            int bottomLeftValue, int bottomMiddleValue, int bottomRightValue,
            int factor, int offset)
        {
            if (factor == 0)
                return pixel;

            var newPixel = (double)((((topLeft * topLeftValue) +
                            (topMiddle * topMiddleValue) +
                            (topRight * topRightValue) +
                            (middleLeft * middleLeftValue) +
                            (pixel * pixelValue) +
                            (middleRight * middleRightValue) +
                            (bottomRight * bottomLeftValue) +
                            (bottomMiddle * bottomMiddleValue) +
                            (bottomRight * bottomRightValue))
                            / factor) + offset);

            if (newPixel < 0) newPixel = 0;
            if (newPixel > 255) newPixel = 255;

            return (ushort)newPixel;
        }
    }


    public static class ImageFilterExtensions
    {
        /// <summary>
        /// Applies Gauss filter to an image.
        /// </summary>
        /// <param name="image">The image to modify.</param>
        /// <returns>Returns the smoothed image.</returns>
        public static Bitmap ApplyGaussFilter(this ImageFilter imageFilter, Bitmap image)
        {
            var memory = CreateSimpleMemory(
                image,
                1, 2, 1,
                2, 4, 2,
                1, 2, 1,
                16);
            imageFilter.FilterImage(memory);
            return CreateImage(memory, image);
        }

        /// <summary>
        /// Applies Sobel filter to the image.
        /// </summary>
        /// <param name="image">The image to modify.</param>
        /// <returns>Returns the edge map of the image.</returns>
        public static Bitmap ApplySobelFilter(this ImageFilter imageFilter, Bitmap image)
        {
            var memory = CreateSimpleMemory(
                image,
                1, 2, 1,
                0, 0, 0,
                -1, -2, -1);
            imageFilter.FilterImage(memory);
            return CreateImage(memory, image);
        }

        /// <summary>
        /// Applies a horizontal edge detection filter to the image.
        /// </summary>
        /// <param name="image">The image to modify.</param>
        /// <returns>Returns the edge map of the image containing only horizontal edges.</returns>
        public static Bitmap DetectHorizontalEdges(this ImageFilter imageFilter, Bitmap image)
        {
            var memory = CreateSimpleMemory(
                image,
                1, 1, 1,
                0, 0, 0,
                -1, -1, -1);
            imageFilter.FilterImage(memory);
            return CreateImage(memory, image);
        }

        /// <summary>
        /// Applies a vertical edge detection filter to the image.
        /// </summary>
        /// <param name="image">The image to modify.</param>
        /// <returns>Returns the edge map of the image containing only vertical edges.</returns>
        public static Bitmap DetectVerticalEdges(this ImageFilter imageFilter, Bitmap image)
        {
            var memory = CreateSimpleMemory(
                image,
                1, 0, -1,
                1, 0, -1,
                1, 0, -1);
            imageFilter.FilterImage(memory);
            return CreateImage(memory, image);
        }


        /// <summary>
        /// Creates a <see cref="SimpleMemory"/> instance that stores the image.
        /// </summary>
        /// <param name="image">The image to process.</param>
        /// <param name="topLeft">Top left value.</param>
        /// <param name="topMiddle">Top middle value.</param>
        /// <param name="topRight">Top right value.</param>
        /// <param name="middleLeft">Middle left value.</param>
        /// <param name="pixel">The current pixel value.</param>
        /// <param name="middleRight">Middle right value.</param>
        /// <param name="bottomLeft">Bottom left value.</param>
        /// <param name="bottomMiddle">Bottom middle value.</param>
        /// <param name="bottomRight">Bottom right value.</param>
        /// <param name="factor">The value to devide the summed matrix values with.</param>
        /// <param name="offset">Offset value added to the result.</param>
        /// <returns>The instance of the created <see cref="SimpleMemory"/>.</returns>
        private static SimpleMemory CreateSimpleMemory(
            Bitmap image,
            int topLeft, int topMiddle, int topRight,
            int middleLeft, int pixel, int middleRight,
            int bottomLeft, int bottomMiddle, int bottomRight,
            int factor = 1, int offset = 0)
        {
            var memory = new SimpleMemory(image.Width * image.Height * 6 + 13);

            memory.WriteUInt32(ImageFilter.FilterImage_ImageWidthIndex, (uint)image.Width);
            memory.WriteUInt32(ImageFilter.FilterImage_ImageHeightIndex, (uint)image.Height);
            memory.WriteInt32(ImageFilter.FilterImage_TopLeftIndex, topLeft);
            memory.WriteInt32(ImageFilter.FilterImage_TopMiddleIndex, topMiddle);
            memory.WriteInt32(ImageFilter.FilterImage_TopRightIndex, topRight);
            memory.WriteInt32(ImageFilter.FilterImage_MiddleLeftIndex, middleLeft);
            memory.WriteInt32(ImageFilter.FilterImage_PixelIndex, pixel);
            memory.WriteInt32(ImageFilter.FilterImage_MiddleRightIndex, middleRight);
            memory.WriteInt32(ImageFilter.FilterImage_BottomLeftIndex, bottomLeft);
            memory.WriteInt32(ImageFilter.FilterImage_BottomMiddleIndex, bottomMiddle);
            memory.WriteInt32(ImageFilter.FilterImage_BottomRightIndex, bottomRight);
            memory.WriteInt32(ImageFilter.FilterImage_FactorIndex, factor);
            memory.WriteInt32(ImageFilter.FilterImage_OffsetIndex, offset);

            int size = image.Width * image.Height;

            for (int x = 0; x < image.Height; x++)
            {
                for (int y = 0; y < image.Width; y++)
                {
                    var pixelValue = image.GetPixel(y, x);

                    memory.WriteUInt32((x * image.Width + y) * 3 + ImageFilter.FilterImage_ImageStartIndex, pixelValue.R);
                    memory.WriteUInt32((x * image.Width + y) * 3 + 1 + ImageFilter.FilterImage_ImageStartIndex, pixelValue.G);
                    memory.WriteUInt32((x * image.Width + y) * 3 + 2 + ImageFilter.FilterImage_ImageStartIndex, pixelValue.B);

                    memory.WriteUInt32((x * image.Width + y) * 3 + (size * 3) + ImageFilter.FilterImage_ImageStartIndex, pixelValue.R);
                    memory.WriteUInt32((x * image.Width + y) * 3 + 1 + (size * 3) + ImageFilter.FilterImage_ImageStartIndex, pixelValue.G);
                    memory.WriteUInt32((x * image.Width + y) * 3 + 2 + (size * 3) + ImageFilter.FilterImage_ImageStartIndex, pixelValue.B);
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
            var newImage = new Bitmap(image);

            int r, g, b;

            for (int x = 0; x < newImage.Height; x++)
            {
                for (int y = 0; y < newImage.Width; y++)
                {
                    r = memory.ReadInt32((x * newImage.Width + y) * 3 + ImageFilter.FilterImage_ImageStartIndex);
                    g = memory.ReadInt32((x * newImage.Width + y) * 3 + 1 + ImageFilter.FilterImage_ImageStartIndex);
                    b = memory.ReadInt32((x * newImage.Width + y) * 3 + 2 + ImageFilter.FilterImage_ImageStartIndex);

                    newImage.SetPixel(y, x, Color.FromArgb(r, g, b));
                }
            }

            return newImage;
        }
    }
}
