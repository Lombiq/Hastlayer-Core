using NUnit.Framework;

namespace Unum.Tests
{
    [TestFixture]
    public class UnumEnvironmentTests
    {
        private Hast.Common.Numerics.Unum.Unum _unum_3_2;
        private Hast.Common.Numerics.Unum.Unum _unum_3_4;


        [SetUp]
        public void Init()
        {
            _unum_3_2 = new Hast.Common.Numerics.Unum.Unum(3, 2);
            _unum_3_4 = new Hast.Common.Numerics.Unum.Unum(3, 4);
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
            Assert.AreEqual(0x20, _unum_3_2.UncertaintyBitMask);
            Assert.AreEqual(0x80, _unum_3_4.UncertaintyBitMask);
        }

        [Test]
        public void PositiveInfinityIsCorrect()
        {
            Assert.AreEqual(0x3FFDF, _unum_3_2.PositiveInfinity); // 0  1111 1111  1111  0 111 11
            //Assert.AreEqual(0xFFFFFF7F, _unum_3_4.PositiveInfinity); // 0  1111 1111  1111 1111 1111 1111  0 111 1111
        }

        [Test]
        public void NegativeInfinityIsCorrect()
        {
            Assert.AreEqual(0x7FFDF, _unum_3_2.NegativeInfinity); // 1  1111 1111  1111  0 111 11
        }

        [Test]
        public void QuietNotANumberIsCorrect()
        {
            Assert.AreEqual(0x3FFFF, _unum_3_2.QuietNotANumber); // 0  1111 1111  1111  1 111 11
        }

        [Test]
        public void SignalingNotANumberIsCorrect()
        {
            Assert.AreEqual(0x7FFFF, _unum_3_2.SignalingNotANumber); // 1  1111 1111  1111  1 111 11
        }

        [Test]
        public void LargestPositiveIsCorrect()
        {
            Assert.AreEqual(0x3FF9F, _unum_3_2.LargestPositive); // 0 1111 1111  1110  0 111 11
        }

        [Test]
        public void SmallestPositiveIsCorrect()
        {
            Assert.AreEqual(0x5F, _unum_3_2.SmallestPositive); // 0 0000 0000  0001  0 111 11
        }

        [Test]
        public void LargestNegativeIsCorrect()
        {
            Assert.AreEqual(0x7FF9F, _unum_3_2.LargestNegative); // 1  1111 1111  1110  0 111 11
        }
    }
}
