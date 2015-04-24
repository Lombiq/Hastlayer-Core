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
        private int _imageHeight;
        private int _imageWidth;
        private double _contrastValue;


        /// <summary>
        /// Creates an instance of <see cref="ImageContrast"/> class.
        /// </summary>
        /// <param name="imageHeight">The height of the image in pixels.</param>
        /// <param name="imageWidth">The width of the image in pixels.</param>
        /// <param name="contrastValue">The difference in contrast.</param>
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


        /// <summary>
        /// Changes the contrast of an image.
        /// </summary>
        /// <param name="memory">The <see cref="SimpleMemory"/> object representing the accessible memory space.</param>
        public virtual void ChangeContrast(SimpleMemory memory)
        {
            int pixel = 0;

            for (int i = 0; i < _imageHeight * _imageWidth * 3; i++)
            {
                pixel = memory.ReadInt32(i);
                memory.WriteInt32(i, WorkUpPixel(pixel));
            }
        }


        /// <summary>
        /// Makes the required changes on the selected pixel.
        /// </summary>
        /// <param name="pixel">The current pixel value.</param>
        /// <returns>The pixel value after changing the contrast.</returns>
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
}
