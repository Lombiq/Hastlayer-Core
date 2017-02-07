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
            var masksAndPositions = new Tuple<BitMask, uint>[]
            {
                Tuple.Create(new BitMask(32, false), (uint)0),
                Tuple.Create(new BitMask(32, false), (uint)0),
                Tuple.Create(new BitMask(32, false), (uint)0),
                Tuple.Create(new BitMask(32, false), (uint)0),
                Tuple.Create(new BitMask(32, false), (uint)0),
                Tuple.Create(new BitMask(33, false), (uint)0),
                Tuple.Create(new BitMask(100, false), (uint)0),
                Tuple.Create(new BitMask(100, false), (uint)0)
            };

            for (int i = 0; i < masksAndPositions.Length; i++)
            {
                var segment = masksAndPositions[i].Item2 / 32;
                var position = masksAndPositions[i].Item2 % 32;

                Assert.AreEqual(1, BitMask.SetOne(masksAndPositions[i].Item1, masksAndPositions[i].Item2).Segments[segment] >> (int)position,
                    $"Position: {masksAndPositions[i]}: Segment {segment}, Position: {position}");
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

        [Test]
        public void BitMaskIntegerAdditionIsCorrect()
        {
            Assert.AreEqual(1, (new BitMask(0) + 1).Segments[0]);
            Assert.AreEqual(0xFFFF << 1, (new BitMask(0xFFFF) + 0xFFFF).Segments[0]);
            Assert.AreEqual(0xFFFFFFFF, (new BitMask(0xFFFFFFFE) + 1).Segments[0]);
            Assert.AreEqual(0xFFFFFFFF, (new BitMask(0xEFFFFFFF) + 0x10000000).Segments[0]);
            Assert.AreEqual(0, (new BitMask(0xFFFFFFFF) + 1).Segments[0]);

            Assert.AreEqual(new BitMask(65, 0, 0, 1), new BitMask(65, 0xFFFFFFFF, 0xFFFFFFFF, 0) + 1);
        }
    }
}
