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
            //Change image contrast

            var memory = CreateSimpleMemory(image);
            imageContrast = new ImageContrast(image.Height, image.Width, contrast);
            imageContrast.ChangeContrast(memory);
            return CreateImage(memory, image);
        }

        public static Bitmap GetGaussImage(this ImageFilter imageFilter, Bitmap image)
        {
            //Gauss smoothing

            var memory = CreateSimpleMemory(image, true);
            imageFilter = new ImageFilter(image.Height, image.Width);
            imageFilter.SetMatrixValues(
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

            var memory = CreateSimpleMemory(image, true);
            imageFilter = new ImageFilter(image.Height, image.Width);
            imageFilter.SetMatrixValues(
                1, 2, 1,
                0, 0, 0,
                -1, -2, -1,
                16);
            imageFilter.FilterImage(memory);
            return CreateImage(memory, image);
        }

        public static Bitmap GetHorizontalEdges(this ImageFilter imageFilter, Bitmap image)
        {
            //Horizontal edge detection

            var memory = CreateSimpleMemory(image, true);
            imageFilter = new ImageFilter(image.Height, image.Width);
            imageFilter.SetMatrixValues(
                1, 1, 1,
                0, 0, 0,
                -1, -1, -1);
            imageFilter.FilterImage(memory);
            return CreateImage(memory, image);
        }

        public static Bitmap GetVerticalEdges(this ImageFilter imageFilter, Bitmap image)
        {
            //Vertical edge detection

            var memory = CreateSimpleMemory(image, true);
            imageFilter = new ImageFilter(image.Height, image.Width);
            imageFilter.SetMatrixValues(
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
        /// <param name="cloneImage">Is image cloning required?</param>
        /// <returns>The instance of the created <see cref="SimpleMemory"/>.</returns>
        private static SimpleMemory CreateSimpleMemory(Bitmap image, bool cloneImage = false)
        {
            SimpleMemory memory = null;

            if (cloneImage)
                memory = new SimpleMemory(image.Width * image.Height * 6);
            else
                memory = new SimpleMemory(image.Width * image.Height * 3);

            for (int x = 0; x < image.Height; x++)
            {
                for (int y = 0; y < image.Width; y++)
                {
                    var pixel = image.GetPixel(y, x);

                    memory.WriteInt32((x * image.Width + y) * 3, pixel.R);
                    memory.WriteInt32((x * image.Width + y) * 3 + 1, pixel.G);
                    memory.WriteInt32((x * image.Width + y) * 3 + 2, pixel.B);

                    if (cloneImage)
                    {
                        int size = image.Width * image.Height;

                        memory.WriteInt32((x * image.Width + y) * 3 + (size * 3), pixel.R);
                        memory.WriteInt32((x * image.Width + y) * 3 + 1 + (size * 3), pixel.G);
                        memory.WriteInt32((x * image.Width + y) * 3 + 2 + (size * 3), pixel.B);
                    }
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
            int r, g, b;

            for (int x = 0; x < image.Height; x++)
            {
                for (int y = 0; y < image.Width; y++)
                {
                    r = memory.ReadInt32((x * image.Width + y) * 3);
                    g = memory.ReadInt32((x * image.Width + y) * 3 + 1);
                    b = memory.ReadInt32((x * image.Width + y) * 3 + 2);

                    image.SetPixel(y, x, Color.FromArgb(r, g, b));
                }
            }

            return image;
        }
    }
}
