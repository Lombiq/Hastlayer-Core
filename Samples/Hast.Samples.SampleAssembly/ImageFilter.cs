using Hast.Transformer.SimpleMemory;
using System;
using System.Collections.Generic;
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
        private int _imageHeight;
        private int _imageWidth;

        private int _topLeft;
        private int _topMiddle;
        private int _topRight;
        private int _middleLeft;
        private int _pixel;
        private int _middleRight;
        private int _bottomLeft;
        private int _bottomMiddle;
        private int _bottomRight;

        private int _offset;
        private int _factor;

        private int _currentIndex;


        /// <summary>
        /// Creates an instance of <see cref="ImageFilter"/> class.
        /// </summary>
        /// <param name="imageHeight">The height of the image in pixels.</param>
        /// <param name="imageWidth">The width of the image in pixels.</param>
        /// <param name="baseValue">All matrix values will be set to this value.</param>
        public ImageFilter(int imageHeight, int imageWidth, int baseValue = 1)
        {
            _imageHeight = imageHeight;
            _imageWidth = imageWidth;

            _topLeft = _topMiddle = _topRight = _middleLeft = _pixel = _middleRight = _bottomLeft = _bottomMiddle = _bottomRight = baseValue;

            _offset = 0;
            _factor = 1;

            _currentIndex = _imageWidth + 1;
        }


        /// <summary>
        /// Sets the values of a 3x3 matrix.
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
        /// <param name="factor">The factor to divide the result with.</param>
        /// <param name="offset">A final offset value.</param>
        public void SetMatrixValues(
            int topLeft, int topMiddle, int topRight,
            int middleLeft, int pixel, int middleRight,
            int bottomLeft, int bottomMiddle, int bottomRight,
            int factor = 1, int offset = 0)
        {
            _topLeft = topLeft;
            _topMiddle = topMiddle;
            _topRight = topRight;
            _middleLeft = middleLeft;
            _pixel = pixel;
            _middleRight = middleRight;
            _bottomLeft = bottomLeft;
            _bottomMiddle = bottomMiddle;
            _bottomRight = bottomRight;
            _factor = factor;
            _offset = offset;
        }

        /// <summary>
        /// Makes the changes according to the matrix on the image.
        /// </summary>
        /// <param name="memory">The <see cref="SimpleMemory"/> object representing the accessible memory space.</param>
        public virtual void FilterImage(SimpleMemory memory)
        {
            _currentIndex = _imageWidth + 1;

            int topLeft = 0;
            int topMiddle = 0;
            int topRight = 0;
            int middleLeft = 0;
            int pixel = 0;
            int middleRight = 0;
            int bottomLeft = 0;
            int bottomMiddle = 0;
            int bottomRight = 0;

            int pixelCountHelper = _imageHeight * _imageWidth * 3;
            int imageWidthHelper = _imageWidth * 3;

            for (int x = 1; x < _imageHeight - 1; x++)
            {
                for (int y = 3; y < imageWidthHelper - 3; y++)
                {
                    topLeft = memory.ReadInt32((ulong)(x * imageWidthHelper + y + pixelCountHelper - imageWidthHelper - 3));
                    topMiddle = memory.ReadInt32((ulong)(x * imageWidthHelper + y + pixelCountHelper - imageWidthHelper));
                    topRight = memory.ReadInt32((ulong)(x * imageWidthHelper + y + pixelCountHelper - imageWidthHelper + 3));
                    middleLeft = memory.ReadInt32((ulong)(x * imageWidthHelper + y + pixelCountHelper - 3));
                    pixel = memory.ReadInt32((ulong)(x * imageWidthHelper + y + pixelCountHelper));
                    middleRight = memory.ReadInt32((ulong)(x * imageWidthHelper + y + pixelCountHelper + 3));
                    bottomLeft = memory.ReadInt32((ulong)(x * imageWidthHelper + y + pixelCountHelper + imageWidthHelper - 3));
                    bottomMiddle = memory.ReadInt32((ulong)(x * imageWidthHelper + y + pixelCountHelper + imageWidthHelper));
                    bottomRight = memory.ReadInt32((ulong)(x * imageWidthHelper + y + pixelCountHelper + imageWidthHelper + 3));

                    memory.WriteInt32((ulong)(x * imageWidthHelper + y), WorkUpMatrix(
                        topLeft, topMiddle, topRight, 
                        middleLeft, pixel, middleRight, 
                        bottomLeft, bottomMiddle, bottomRight));
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
        /// <returns>Returns the value of the filtered pixel.</returns>
        private int WorkUpMatrix(
            int topLeft, int topMiddle, int topRight,
            int middleLeft, int pixel, int middleRight,
            int bottomLeft, int bottomMiddle, int bottomRight)
        {
            if (_factor == 0)
                return pixel;

            var newPixel = (double)((((topLeft * _topLeft) +
                            (topMiddle * _topMiddle) +
                            (topRight * _topRight) +
                            (middleLeft * _middleLeft) +
                            (pixel * _pixel) +
                            (middleRight * _middleRight) +
                            (bottomRight * _bottomLeft) +
                            (bottomMiddle * _bottomMiddle) +
                            (bottomRight * _bottomRight))
                            / _factor) + _offset);

            if (newPixel < 0) newPixel = 0;
            if (newPixel > 255) newPixel = 255;

            return (int)newPixel;
        }
    }
}
