using Hast.Common.Numerics;
using Hast.Common.Numerics.Unum;
using NUnit.Framework;

namespace Hast.Common.Tests
{
    [TestFixture]
    public class UnumEnvironmentTests
    {
        private Unum _unum_3_2;
        private Unum _unum_3_4;


        [SetUp]
        public void Init()
        {
            _unum_3_2 = new Unum(3, 2);
            _unum_3_4 = new Unum(3, 4);
        }

        [TestFixtureTearDown]
        public void Clean() { }


        [Test]
        public void UnumExponentSizeSizeIsCorrect()
        {
            Assert.AreEqual(3, _unum_3_2.ExponentSizeSize);
            Assert.AreEqual(3, _unum_3_4.ExponentSizeSize);
        }

        [Test]
        public void UnumFractionSizeSizeIsCorrect()
        {
            Assert.AreEqual(2, _unum_3_2.FractionSizeSize);
            Assert.AreEqual(4, _unum_3_4.FractionSizeSize);
        }

        [Test]
        public void UnumTagSizeIsCorrect()
        {
            Assert.AreEqual(6, _unum_3_2.UnumTagSize);
            Assert.AreEqual(8, _unum_3_4.UnumTagSize);
        }

        [Test]
        public void UnumSizeIsCorrect()
        {
            Assert.AreEqual(19, _unum_3_2.Size);
            Assert.AreEqual(33, _unum_3_4.Size);
        }

        [Test]
        public void UnumUncertaintyBitMaskIsCorrect()
        {
            // 0  0000 0000  0000  1 000 00
            Assert.AreEqual(new BitMask(new uint[] { 0x20 }, _unum_3_2.Size), _unum_3_2.UncertaintyBitMask,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.UncertaintyBitMask)));

            // 0  0000 0000  0000 0000 0000 0000  1 000 0000
            Assert.AreEqual(new BitMask(new uint[] { 0x80, 0 }, _unum_3_4.Size), _unum_3_4.UncertaintyBitMask,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.UncertaintyBitMask)));
        }

        [Test]
        public void UnumExponentSizeMaskIsCorrect()
        {
            // 0  0000 0000  0000  0 111 00
            Assert.AreEqual(new BitMask(new uint[] { 0x1C }, _unum_3_2.Size), _unum_3_2.ExponentSizeMask,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.ExponentSizeMask)));

            // 0  0000 0000  0000 0000 0000 0000  0 111 0000
            Assert.AreEqual(new BitMask(new uint[] { 0x70, 0 }, _unum_3_4.Size), _unum_3_4.ExponentSizeMask,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.ExponentSizeMask)));
        }

        [Test]
        public void UnumFractionSizeMaskIsCorrect()
        {
            // 0  0000 0000  0000  0 000 11
            Assert.AreEqual(new BitMask(new uint[] { 3 }, _unum_3_2.Size), _unum_3_2.FractionSizeMask,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.FractionSizeMask)));

            // 0  0000 0000  0000 0000 0000 0000  0 000 1111
            Assert.AreEqual(new BitMask(new uint[] { 0xF, 0 }, _unum_3_4.Size), _unum_3_4.FractionSizeMask,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.FractionSizeMask)));
        }

        [Test]
        public void UnumExponentAndFractionSizeMaskIsCorrect()
        {
            // 0  0000 0000  0000  0 111 11
            Assert.AreEqual(new BitMask(new uint[] { 0x1F }, _unum_3_2.Size), _unum_3_2.ExponentAndFractionSizeMask,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.ExponentAndFractionSizeMask)));

            // 0  0000 0000  0000 0000 0000 0000  0 111 1111
            Assert.AreEqual(new BitMask(new uint[] { 0x7F, 0 }, _unum_3_4.Size), _unum_3_4.ExponentAndFractionSizeMask,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.ExponentAndFractionSizeMask)));
        }

        [Test]
        public void UnumTagMaskIsCorrect()
        {
            // 0  0000 0000  0000  1 111 11
            Assert.AreEqual(new BitMask(new uint[] { 0x3F }, _unum_3_2.Size), _unum_3_2.UnumTagMask,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.UnumTagMask)));

            // 0  0000 0000  0000 0000 0000 0000  1 111 1111
            Assert.AreEqual(new BitMask(new uint[] { 0xFF, 0 }, _unum_3_4.Size), _unum_3_4.UnumTagMask,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.UnumTagMask)));
        }

        [Test]
        public void UnumSignBitMaskIsCorrect()
        {
            // 1  0000 0000  0000  0 000 00
            Assert.AreEqual(new BitMask(new uint[] { 0x40000 }, _unum_3_2.Size), _unum_3_2.SignBitMask,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.SignBitMask)));

            // 1  0000 0000  0000 0000 0000 0000  0 000 0000
            Assert.AreEqual(new BitMask(new uint[] { 0, 1 }, _unum_3_4.Size), _unum_3_4.SignBitMask,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.SignBitMask)));
        }

        [Test]
        public void UnumPositiveInfinityIsCorrect()
        {
            // 0  1111 1111  1111  0 111 11
            Assert.AreEqual(new BitMask(new uint[] { 0x3FFDF }, _unum_3_2.Size), _unum_3_2.PositiveInfinity,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.PositiveInfinity)));

            // 0  1111 1111  1111 1111 1111 1111  0 111 1111
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFF7F, 0 }, _unum_3_4.Size), _unum_3_4.PositiveInfinity,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.PositiveInfinity)));
        }

        [Test]
        public void UnumNegativeInfinityIsCorrect()
        {
            // 1  1111 1111  1111  0 111 11
            Assert.AreEqual(new BitMask(new uint[] { 0x7FFDF }, _unum_3_2.Size), _unum_3_2.NegativeInfinity,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.NegativeInfinity)));

            // 1  1111 1111  1111 1111 1111 1111  0 111 1111
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFF7F, 1 }, _unum_3_4.Size), _unum_3_4.NegativeInfinity,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.NegativeInfinity)));
        }

        [Test]
        public void UnumQuietNotANumberIsCorrect()
        {
            // 0  1111 1111  1111  1 111 11
            Assert.AreEqual(new BitMask(new uint[] { 0x3FFFF }, _unum_3_2.Size), _unum_3_2.QuietNotANumber,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.QuietNotANumber)));

            // 0  1111 1111  1111 1111 1111 1111  1 111 1111
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFFFF, 0 }, _unum_3_4.Size), _unum_3_4.QuietNotANumber,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.QuietNotANumber)));
        }

        [Test]
        public void UnumSignalingNotANumberIsCorrect()
        {
            // 1  1111 1111  1111  1 111 11
            Assert.AreEqual(new BitMask(new uint[] { 0x7FFFF }, _unum_3_2.Size), _unum_3_2.SignalingNotANumber,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.SignalingNotANumber)));

            // 1  1111 1111  1111 1111 1111 1111  1 111 1111
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFFFF, 1 }, _unum_3_4.Size), _unum_3_4.SignalingNotANumber,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.SignalingNotANumber)));
        }

        [Test]
        public void UnumLargestPositiveIsCorrect()
        {
            // 0  1111 1111  1110  0 111 11
            Assert.AreEqual(new BitMask(new uint[] { 0x3FF9F }, _unum_3_2.Size), _unum_3_2.LargestPositive,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.LargestPositive)));

            // 0  1111 1111  1111 1111 1111 1110  0 111 1111
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFE7F, 0 }, _unum_3_4.Size), _unum_3_4.LargestPositive,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.LargestPositive)));
        }

        [Test]
        public void UnumSmallestPositiveIsCorrect()
        {
            // 0  0000 0000  0001  0 111 11
            Assert.AreEqual(new BitMask(new uint[] { 0x5F }, _unum_3_2.Size), _unum_3_2.SmallestPositive,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.SmallestPositive)));

            // 0  0000 0000  0000 0000 0000 0001  0 111 1111
            Assert.AreEqual(new BitMask(new uint[] { 0x17F, 0 }, _unum_3_4.Size), _unum_3_4.SmallestPositive,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.SmallestPositive)));
        }

        [Test]
        public void UnumLargestNegativeIsCorrect()
        {
            // 1  1111 1111  1110  0 111 11
            Assert.AreEqual(new BitMask(new uint[] { 0x7FF9F }, _unum_3_2.Size), _unum_3_2.LargestNegative,
                TestFailureMessageBuilder(_unum_3_2, nameof(_unum_3_2.LargestNegative)));

            // 1  1111 1111  1111 1111 1111 1110  0 111 1111
            Assert.AreEqual(new BitMask(new uint[] { 0xFFFFFE7F, 1 }, _unum_3_4.Size), _unum_3_4.LargestNegative,
                TestFailureMessageBuilder(_unum_3_4, nameof(_unum_3_4.LargestNegative)));
        }


        private string TestFailureMessageBuilder(Unum unum, string propertyName) =>
            $"Testing the \"{propertyName}\" property of the Unum ({unum.ExponentSizeSize}, {unum.FractionSizeSize}) environment failed.";
    }
}
