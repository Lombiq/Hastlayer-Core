using Hast.Common.Numerics;
using NUnit.Framework;
using System;

namespace Hast.Common.Tests
{
    [TestFixture]
    public class BitMaskTests
    {
        [Test]
        public void BitMaskSegmentCountIsCorrectlyCalculatedFromSize()
        {
            var masks = new BitMask[]
            {
                new BitMask(15, false), new BitMask(31, false), new BitMask(32, false), new BitMask(33, false),
                new BitMask(1023, false), new BitMask(1024, false), new BitMask(1025, false)
            };

            foreach (var mask in masks) Assert.AreEqual((int)Math.Ceiling((double)mask.Size / 32), mask.SegmentCount);
        }

        [Test]
        public void BitMaskSizeIsCorrectlySetWithSegments()
        {
            var masks = new BitMask[]
            {
                new BitMask(15, 1, 1), new BitMask(15, 1, 1), new BitMask(40, 1), new BitMask(40, 1, 1),
                new BitMask(40, 1, 1, 1), new BitMask(0, 1), new BitMask(0, 1, 1), new BitMask(0, 1, 1, 1)
            };

            foreach (var mask in masks)
                Assert.That((mask.Size == 0 && mask.SegmentCount == 1) ||
                    mask.Size > (mask.SegmentCount - 1) * 32 && mask.Size <= mask.SegmentCount * 32);
        }

        [Test]
        public void BitMaskSetOneIsCorrect()
        {
            var masks = new BitMask[]
            {
                new BitMask(32, false), new BitMask(32, false), new BitMask(32, false), new BitMask(32, false),
                new BitMask(32, false), new BitMask(33, false), new BitMask(100, false), new BitMask(100, false)
            };
            var positions = new uint[]
            {
                0, 1, 5, 10,
                31, 32, 33, 95
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var segment = positions[i] / 32;
                var position = positions[i] % 32;

                Assert.AreEqual(1, BitMask.SetOne(masks[i], positions[i]).Segments[segment] >> (int)position,
                    $"Position: {positions[i]}: Segment {segment}, Position: {position}");
            }
        }

        [Test]
        public void BitMaskConstructorCorrectlyCopiesBitMask()
        {
            var masks = new BitMask[]
            {
                new BitMask(), new BitMask(42, 0x42, 0x42), new BitMask(88, 0x88, 0x88, 0x88)
            };

            foreach (var mask in masks) Assert.AreEqual(mask, new BitMask(mask));
        }
    }
}
