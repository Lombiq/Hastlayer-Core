using Hast.Transformer.SimpleMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Samples.SampleAssembly
{
    public class ImageProcessorFilter
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


        public ImageProcessorFilter(int imageHeight, int imageWidth, int baseValue = 1)
        {
            _imageHeight = imageHeight;
            _imageWidth = imageWidth;

            _topLeft = _topMiddle = _topRight = _middleLeft = _pixel = _middleRight = _bottomLeft = _bottomMiddle = _bottomRight = baseValue;

            _offset = 0;
            _factor = 1;

            _currentIndex = _imageWidth + 1;
        }


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

        public virtual void ProcessImage(SimpleMemory memory)
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
