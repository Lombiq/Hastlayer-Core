﻿using Hast.Transformer.SimpleMemory;
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
}
