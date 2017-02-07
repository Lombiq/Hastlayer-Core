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
        public void ExponentSizeSizeIsCorrect()
        {
            Assert.AreEqual(3, _unum_3_2.ExponentSizeSize);
            Assert.AreEqual(3, _unum_3_4.ExponentSizeSize);
        }

        [Test]
        public void FractionSizeSizeIsCorrect()
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
        public void UncertaintyBitMaskIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x20 == _unum_3_2.UncertaintyBitMask); // 0  0000 0000  0000  1 000 00
            Assert.That(new BitMask(_unum_3_4.Size, false) + 0x80 == _unum_3_4.UncertaintyBitMask); // 0  0000 0000  0000 0000 0000 0000  1 000 0000
        }

        [Test]
        public void ExponentSizeMaskIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x1C == _unum_3_2.ExponentSizeMask); // 0  0000 0000  0000  0 111 00
            Assert.That(new BitMask(_unum_3_4.Size, false) + 0x70 == _unum_3_4.ExponentSizeMask); // 0  0000 0000  0000 0000 0000 0000  0 111 0000
        }

        [Test]
        public void FractionSizeMaskIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x3 == _unum_3_2.FractionSizeMask); // 0  0000 0000  0000  0 000 11
            Assert.That(new BitMask(_unum_3_4.Size, false) + 0xF == _unum_3_4.FractionSizeMask); // 0  0000 0000  0000 0000 0000 0000  0 000 1111
        }

        [Test]
        public void ExponentAndFractionSizeMaskIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x1F == _unum_3_2.ExponentAndFractionSizeMask); // 0  0000 0000  0000  0 111 11
            Assert.That(new BitMask(_unum_3_4.Size, false) + 0x7F == _unum_3_4.ExponentAndFractionSizeMask); // 0  0000 0000  0000 0000 0000 0000  0 111 1111
        }

        [Test]
        public void UnumTagMaskIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x3F == _unum_3_2.UnumTagMask); // 0  0000 0000  0000  1 111 11
            Assert.That(new BitMask(_unum_3_4.Size, false) + 0xFF == _unum_3_4.UnumTagMask); // 0  0000 0000  0000 0000 0000 0000  1 111 1111
        }

        [Test]
        public void SignBitMaskIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x40000 == _unum_3_2.SignBitMask); // 1  0000 0000  0000  0 000 00
            Assert.That(new BitMask(_unum_3_4.Size, false).Segments[0] == _unum_3_4.SignBitMask.Segments[0] &&
                _unum_3_4.SignBitMask.Segments[1] == 1); // 1  0000 0000  0000 0000 0000 0000  0 000 0000
        }

        [Test]
        public void PositiveInfinityIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x3FFDF == _unum_3_2.PositiveInfinity); // 0  1111 1111  1111  0 111 11
            Assert.That(new BitMask(_unum_3_4.Size, false) + 0xFFFFFF7F == _unum_3_4.PositiveInfinity); // 0  1111 1111  1111 1111 1111 1111  0 111 1111
        }

        [Test]
        public void NegativeInfinityIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x7FFDF == _unum_3_2.NegativeInfinity); // 1  1111 1111  1111  0 111 11
            Assert.That((new BitMask(_unum_3_4.Size, false) + 0xFFFFFF7F).Segments[0] == _unum_3_4.NegativeInfinity.Segments[0] &&
                _unum_3_4.NegativeInfinity.Segments[1] == 1); // 1  1111 1111  1111 1111 1111 1111  0 111 1111
        }

        [Test]
        public void QuietNotANumberIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x3FFFF == _unum_3_2.QuietNotANumber); // 0  1111 1111  1111  1 111 11
            Assert.That((new BitMask(_unum_3_4.Size, false) + 0xFFFFFFFF).Segments[0] == _unum_3_4.QuietNotANumber.Segments[0] &&
                _unum_3_4.QuietNotANumber.Segments[1] == 0); // 0  1111 1111  1111 1111 1111 1111  1 111 1111
        }

        [Test]
        public void SignalingNotANumberIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x7FFFF == _unum_3_2.SignalingNotANumber); // 1  1111 1111  1111  1 111 11
            Assert.That((new BitMask(_unum_3_4.Size, false) + 0xFFFFFFFF).Segments[0] == _unum_3_4.SignalingNotANumber.Segments[0] &&
                _unum_3_4.SignalingNotANumber.Segments[1] == 1); // 1  1111 1111  1111 1111 1111 1111  1 111 1111
        }

        [Test]
        public void LargestPositiveIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x3FF9F == _unum_3_2.LargestPositive); // 0  1111 1111  1110  0 111 11
            Assert.That((new BitMask(_unum_3_4.Size, false) + 0xFFFFFE7F).Segments[0] == _unum_3_4.LargestPositive.Segments[0] &&
                _unum_3_4.LargestPositive.Segments[1] == 0); // 0  1111 1111  1111 1111 1111 1110  0 111 1111
        }

        [Test]
        public void SmallestPositiveIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x5F == _unum_3_2.SmallestPositive); // 0  0000 0000  0001  0 111 11
            Assert.That((new BitMask(_unum_3_4.Size, false) + 0x17F).Segments[0] == _unum_3_4.SmallestPositive.Segments[0] &&
                _unum_3_4.SmallestPositive.Segments[1] == 0); // 0  0000 0000  0000 0000 0000 0001  0 111 1111
        }

        [Test]
        public void LargestNegativeIsCorrect()
        {
            Assert.That(new BitMask(_unum_3_2.Size, false) + 0x7FF9F == _unum_3_2.LargestNegative); // 1  1111 1111  1110  0 111 11
            Assert.That((new BitMask(_unum_3_4.Size, false) + 0xFFFFFE7F).Segments[0] == _unum_3_4.LargestNegative.Segments[0] &&
                _unum_3_4.LargestNegative.Segments[1] == 1); // 1  1111 1111  1111 1111 1111 1110  0 111 1111
        }


        private string TestFailureMessageBuilder(Unum unum, string propertyName)
        {
            return string.Format("Testing the \"{0}\" property of the Unum ({1}, {2}) environment failed.",
                propertyName, unum.ExponentSizeSize, unum.FractionSizeSize);
        }
    }
}
