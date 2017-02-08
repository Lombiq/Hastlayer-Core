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
            var sizesAndSegmentCounts = new Tuple<BitMask, uint>[]
            {
                Tuple.Create(new BitMask(0, false), (uint)0),
                Tuple.Create(new BitMask(31, false), (uint)1),
                Tuple.Create(new BitMask(32, false), (uint)1),
                Tuple.Create(new BitMask(33, false), (uint)2),
                Tuple.Create(new BitMask(1023, false), (uint)32),
                Tuple.Create(new BitMask(1024, false), (uint)32),
                Tuple.Create(new BitMask(1025, false), (uint)33)
            };

            foreach (var item in sizesAndSegmentCounts) Assert.AreEqual(item.Item2, item.Item1.SegmentCount, $"Size: {item.Item1.Size}");
        }

        [Test]
        public void BitMaskSizeIsCorrectlySetWithSegments()
        {
            var sizesAndSegmentCounts = new Tuple<BitMask, uint>[]
            {
                Tuple.Create(new BitMask(new uint[0]), (uint)0),
                Tuple.Create(new BitMask(new uint[] { 1 }), (uint)32),
                Tuple.Create(new BitMask(new uint[] { 2, 2 }), (uint)64),
                Tuple.Create(new BitMask(new uint[] { 3, 3, 3 }), (uint)96),
                Tuple.Create(new BitMask(new uint[] { 4, 4, 4, 4 }), (uint)128),
                Tuple.Create(new BitMask(new uint[] { 0 }, 222), (uint)222)
            };

            foreach (var item in sizesAndSegmentCounts) Assert.AreEqual(item.Item2, item.Item1.Size, $"Mask: {item.Item1}");
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
                new BitMask(), new BitMask(new uint[] { 0x42, 0x42 }), new BitMask(new uint[] { 0x88, 0x88, 0x88 })
            };

            foreach (var mask in masks) Assert.AreEqual(mask, new BitMask(mask));
        }

        [Test]
        public void BitMaskIntegerAdditionIsCorrect()
        {
            Assert.AreEqual(1, (new BitMask(new uint[] { 0 }) + 1).Segments[0]);
            Assert.AreEqual(0x1FFFE, (new BitMask(new uint[] { 0xFFFF }) + 0xFFFF).Segments[0]);
            Assert.AreEqual(0xFFFFFFFF, (new BitMask(new uint[] { 0xFFFFFFFE }) + 1).Segments[0]);
            Assert.AreEqual(0xFFFFFFFF, (new BitMask(new uint[] { 0xEFFFFFFF }) + 0x10000000).Segments[0]);
            Assert.AreEqual(0, (new BitMask(new uint[] { 0xFFFFFFFF }) + 1).Segments[0]);
            Assert.AreEqual(1, (new BitMask(new uint[] { 0xFFFFFFFF }) + 2).Segments[0]);
            Assert.AreEqual(new BitMask(new uint[] { 0, 0, 1 }), new BitMask(new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0 }) + 1);
        }

        [Test]
        public void BitMaskIntegerSubtractionIsCorrect()
        {
            Assert.AreEqual(0, (new BitMask(new uint[] { 1 }) - 1).Segments[0]);
            Assert.AreEqual(0, (new BitMask(new uint[] { 0xFFFFFFFF }) - 0xFFFFFFFF).Segments[0]);
            Assert.AreEqual(1, (new BitMask(new uint[] { 0xFFFFFFFF }) - 0xFFFFFFFE).Segments[0]);
            Assert.AreEqual(0xFFFFFFFF, (new BitMask(new uint[] { 0 }) - 1).Segments[0]);
            Assert.AreEqual(0xFFFFFFFE, (new BitMask(new uint[] { 0 }) - 2).Segments[0]);
            Assert.AreEqual(0xEFFFFFFF, (new BitMask(new uint[] { 0xFFFFFFFF }) - 0x10000000).Segments[0]);
            Assert.AreEqual(0xFFFFFF, (new BitMask(new uint[] { 0x017FFFFF }) - 0x800000).Segments[0]);
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFFFF, 0 }, 33), new BitMask(new uint[] { 0x7FFFFFFF, 1 }, 33) - 0x80000000);
        }

        [Test]
        public void BitMaskAdditionIsCorrect()
        {
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFFFF }),
                            new BitMask(new uint[] { 0x55555555 }) + new BitMask(new uint[] { 0xAAAAAAAA }));
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFFFE, 1 }),
                            new BitMask(new uint[] { 0xFFFFFFFF, 0 }) + new BitMask(new uint[] { 0xFFFFFFFF, 0 }));
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFFFE, 0xFFFFFFFF, 1 }),
                            new BitMask(new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0 }) + new BitMask(new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0 }));
        }

        [Test]
        public void BitMaskSubtractionIsCorrect()
        {
            Assert.AreEqual(new BitMask(new uint[] { 0xAAAAAAAA }),
                            new BitMask(new uint[] { 0xFFFFFFFF }) - new BitMask(new uint[] { 0x55555555 }));
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFFFF, 0 }),
                            new BitMask(new uint[] { 0xFFFFFFFE, 1 }) - new BitMask(new uint[] { 0xFFFFFFFF, 0 }));
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0 }),
                            new BitMask(new uint[] { 0xFFFFFFFE, 0xFFFFFFFF, 1 }) - new BitMask(new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, 0 }));
        }
    }
}