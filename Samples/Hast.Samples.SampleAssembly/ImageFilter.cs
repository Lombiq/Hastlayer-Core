using Hast.Transformer.SimpleMemory;
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
        public const int ImageHeight_Index = 0;
        public const int ImageWidth_Index = 1;
        public const int TopLeft_Index = 2;
        public const int TopMiddle_Index = 3;
        public const int TopRight_Index = 4;
        public const int MiddleLeft_Index = 5;
        public const int Pixel_Index = 6;
        public const int MiddleRight_Index = 7;
        public const int BottomLeft_Index = 8;
        public const int BottomMiddle_Index = 9;
        public const int BottomRight_Index = 10;
        public const int Factor_Index = 11;
        public const int Offset_Index = 12;
        public const int ImageStart_Index = 13;


        /// <summary>
        /// Makes the changes according to the matrix on the image.
        /// </summary>
        /// <param name="memory">The <see cref="SimpleMemory"/> object representing the accessible memory space.</param>
        public virtual void FilterImage(SimpleMemory memory)
        {
            int imageWidth = memory.ReadInt32(ImageWidth_Index);
            int imageHeight = memory.ReadInt32(ImageHeight_Index);

            int factor = memory.ReadInt32(Factor_Index);
            int offset = memory.ReadInt32(Offset_Index);
            int topLeftValue = memory.ReadInt32(TopLeft_Index);
            int topMiddleValue = memory.ReadInt32(TopMiddle_Index);
            int topRightValue = memory.ReadInt32(TopRight_Index);
            int middleLeftValue = memory.ReadInt32(MiddleLeft_Index);
            int pixelValue = memory.ReadInt32(Pixel_Index);
            int middleRightValue = memory.ReadInt32(MiddleRight_Index);
            int bottomLeftValue = memory.ReadInt32(BottomLeft_Index);
            int bottomMiddleValue = memory.ReadInt32(BottomMiddle_Index);
            int bottomRightValue = memory.ReadInt32(BottomRight_Index);

            int topLeft = 0;
            int topMiddle = 0;
            int topRight = 0;
            int middleLeft = 0;
            int pixel = 0;
            int middleRight = 0;
            int bottomLeft = 0;
            int bottomMiddle = 0;
            int bottomRight = 0;

            int pixelCountHelper = imageHeight * imageWidth * 3;
            int imageWidthHelper = imageWidth * 3;

            for (int x = 1; x < imageHeight - 1; x++)
            {
                for (int y = 3; y < imageWidthHelper - 3; y++)
                {
                    topLeft = memory.ReadInt32(x * imageWidthHelper + y + pixelCountHelper - imageWidthHelper - 3 + ImageStart_Index);
                    topMiddle = memory.ReadInt32(x * imageWidthHelper + y + pixelCountHelper - imageWidthHelper + ImageStart_Index);
                    topRight = memory.ReadInt32(x * imageWidthHelper + y + pixelCountHelper - imageWidthHelper + 3 + ImageStart_Index);
                    middleLeft = memory.ReadInt32(x * imageWidthHelper + y + pixelCountHelper - 3 + ImageStart_Index);
                    pixel = memory.ReadInt32(x * imageWidthHelper + y + pixelCountHelper + ImageStart_Index);
                    middleRight = memory.ReadInt32(x * imageWidthHelper + y + pixelCountHelper + 3 + ImageStart_Index);
                    bottomLeft = memory.ReadInt32(x * imageWidthHelper + y + pixelCountHelper + imageWidthHelper - 3 + ImageStart_Index);
                    bottomMiddle = memory.ReadInt32(x * imageWidthHelper + y + pixelCountHelper + imageWidthHelper + ImageStart_Index);
                    bottomRight = memory.ReadInt32(x * imageWidthHelper + y + pixelCountHelper + imageWidthHelper + 3 + ImageStart_Index);

                    memory.WriteInt32(x * imageWidthHelper + y + ImageStart_Index, WorkUpMatrix(
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
        private int WorkUpMatrix(
            int topLeft, int topMiddle, int topRight,
            int middleLeft, int pixel, int middleRight,
            int bottomLeft, int bottomMiddle, int bottomRight,
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

            return (int)newPixel;
        }
    }


    public static class ImageFilterExtensions
    {
        public static Bitmap GetGaussImage(this ImageFilter imageFilter, Bitmap image)
        {
            //Gauss smoothing

            var memory = CreateSimpleMemory(
                image,
                1, 2, 1,
                2, 4, 2,
                1, 2, 1,
                16);
            imageFilter.FilterImage(memory);
            return CreateImage(memory, image);
        }

        public static Bitmap GetSobelImage(this ImageFilter imageFilter, Bitmap image)
        {
            //Sobel edge detection

            var memory = CreateSimpleMemory(
                image,
                1, 2, 1,
                0, 0, 0,
                -1, -2, -1);
            imageFilter.FilterImage(memory);
            return CreateImage(memory, image);
        }

        public static Bitmap GetHorizontalEdges(this ImageFilter imageFilter, Bitmap image)
        {
            //Horizontal edge detection

            var memory = CreateSimpleMemory(
                image,
                1, 1, 1,
                0, 0, 0,
                -1, -1, -1);
            imageFilter.FilterImage(memory);
            return CreateImage(memory, image);
        }

        public static Bitmap GetVerticalEdges(this ImageFilter imageFilter, Bitmap image)
        {
            //Vertical edge detection

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

            memory.WriteInt32(ImageFilter.ImageWidth_Index, image.Width);
            memory.WriteInt32(ImageFilter.ImageHeight_Index, image.Height);
            memory.WriteInt32(ImageFilter.TopLeft_Index, topLeft);
            memory.WriteInt32(ImageFilter.TopMiddle_Index, topMiddle);
            memory.WriteInt32(ImageFilter.TopRight_Index, topRight);
            memory.WriteInt32(ImageFilter.MiddleLeft_Index, middleLeft);
            memory.WriteInt32(ImageFilter.Pixel_Index, pixel);
            memory.WriteInt32(ImageFilter.MiddleRight_Index, middleRight);
            memory.WriteInt32(ImageFilter.BottomLeft_Index, bottomLeft);
            memory.WriteInt32(ImageFilter.BottomMiddle_Index, bottomMiddle);
            memory.WriteInt32(ImageFilter.BottomRight_Index, bottomRight);
            memory.WriteInt32(ImageFilter.Factor_Index, factor);
            memory.WriteInt32(ImageFilter.Offset_Index, offset);

            int size = image.Width * image.Height;

            for (int x = 0; x < image.Height; x++)
            {
                for (int y = 0; y < image.Width; y++)
                {
                    var pixelValue = image.GetPixel(y, x);

                    memory.WriteInt32((x * image.Width + y) * 3 + ImageFilter.ImageStart_Index, pixelValue.R);
                    memory.WriteInt32((x * image.Width + y) * 3 + 1 + ImageFilter.ImageStart_Index, pixelValue.G);
                    memory.WriteInt32((x * image.Width + y) * 3 + 2 + ImageFilter.ImageStart_Index, pixelValue.B);

                    memory.WriteInt32((x * image.Width + y) * 3 + (size * 3) + ImageFilter.ImageStart_Index, pixelValue.R);
                    memory.WriteInt32((x * image.Width + y) * 3 + 1 + (size * 3) + ImageFilter.ImageStart_Index, pixelValue.G);
                    memory.WriteInt32((x * image.Width + y) * 3 + 2 + (size * 3) + ImageFilter.ImageStart_Index, pixelValue.B);
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
                    r = memory.ReadInt32((x * newImage.Width + y) * 3 + ImageFilter.ImageStart_Index);
                    g = memory.ReadInt32((x * newImage.Width + y) * 3 + 1 + ImageFilter.ImageStart_Index);
                    b = memory.ReadInt32((x * newImage.Width + y) * 3 + 2 + ImageFilter.ImageStart_Index);

                    newImage.SetPixel(y, x, Color.FromArgb(r, g, b));
                }
            }

            return newImage;
        }
    }
}
