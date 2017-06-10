using Hast.Common.Numerics;
using Hast.Common.Numerics.Unum;
using NUnit.Framework;


namespace Hast.Common.Tests
{
    [TestFixture]
    public class UnumTests
    {




        [Test]
        public void IsExactIsCorrect()
        {
            // 0  0000 0000  0000  1 000 00
            var bitmask_3_2_uncertain = new BitMask(new uint[] { 0x20 }, 19);
            var unum_3_2_uncertain = new Unum(bitmask_3_2_uncertain, 3, 4);
            Assert.AreEqual(unum_3_2_uncertain.IsExact(), false);

            var bitmask_3_4_uncertain = new BitMask(new uint[] { 0x80, 0 }, 33);
            var unum_3_4_uncertain = new Unum(bitmask_3_4_uncertain, 3, 4);
            Assert.AreEqual(unum_3_4_uncertain.IsExact(), false);
        }
        [Test]
        public void FractionSizeIsCorrect()
        {
            var bitmask_3_2_allOne = new BitMask(19, true);
            var unum_3_2_allOne = new Unum(bitmask_3_2_allOne, 3, 2);
            Assert.AreEqual(unum_3_2_allOne.FractionSize(), 4);

            var bitmask_3_4_allOne = new BitMask(33, true);
            var unum_3_4_allOne = new Unum(bitmask_3_4_allOne, 3, 4);
            Assert.AreEqual(unum_3_4_allOne.FractionSize(), 16);

        }
        [Test]
        public void ExponentSizeIsCorrect()
        {
            var bitmask_3_2_allOne = new BitMask(19, true);
            var unum_3_2_allOne = new Unum(bitmask_3_2_allOne, 3, 2);
            Assert.AreEqual(unum_3_2_allOne.ExponentSize(), 8);


        }

        [Test]
        public void FractionMaskIsCorrect()
        {
            // 0  0000 0000  1111  0000 00
            var bitmask_3_2_allOne = new BitMask(19, true);
            var unum_3_2_allOne = new Unum(bitmask_3_2_allOne, 3, 2);
            var bitmask_3_2_FractionMask = new BitMask(new uint[] { 0x3C0 }, 19);
            Assert.AreEqual(unum_3_2_allOne.FractionMask(), bitmask_3_2_FractionMask);

            // 0  0000 0000  1111 1111 1111 1111  0000 0000
            var bitmask_3_4_allOne = new BitMask(33, true);
            var unum_3_4_allOne = new Unum(bitmask_3_4_allOne, 3, 4);
            var bitmask_3_4_FractionMask = new BitMask(new uint[] { 0xFFFF00 }, 33);
            Assert.AreEqual(unum_3_4_allOne.FractionMask(), bitmask_3_4_FractionMask);

        }
        [Test]
        public void ExponentMaskIsCorrect()
        {
            // 0  1111 1111  0000   0000 00
            var bitmask_3_2_allOne = new BitMask(19, true);
            var unum_3_2_allOne = new Unum(bitmask_3_2_allOne, 3, 2);
            var bitmask_3_2_ExponentMask = new BitMask(new uint[] { 0x3FC00 }, 19);
            Assert.AreEqual(unum_3_2_allOne.ExponentMask() == bitmask_3_2_ExponentMask, true);

            // 0  1111 1111  0000 0000 0000 0000 0000 0000
            var bitmask_3_4_allOne = new BitMask(33, true);
            var unum_3_4_allOne = new Unum(bitmask_3_4_allOne, 3, 4);
            var bitmask_3_4_ExponentMask = new BitMask(new uint[] { 0xFF000000 }, 33);

            Assert.AreEqual(unum_3_4_allOne.ExponentMask() == bitmask_3_4_ExponentMask, true);


        }
        [Test]
        public void ExponentValueWithBiasIsCorrect()
        {
            var bitmask1 = new BitMask(new uint[] { 0xE40 }, 33);
            var unum1 = new Unum(bitmask1, 3, 4);
            Assert.AreEqual(unum1.ExponentValueWithBias(), -8);
        }
        [Test]
        public void FractionWithHiddenBitIsCorrect()
        {
            var bitmask1 = new BitMask(new uint[] { 0xE40 }, 33);
            var bitmask2 = new BitMask(new uint[] { 0x3F22 }, 33);
            var bitmask3 = new BitMask(new uint[] { 0x7E012B }, 33);
            var unum1 = new Unum(bitmask1, 3, 4);
            var unum2 = new Unum(bitmask2, 3, 4);
            var unum3 = new Unum(bitmask3, 3, 4);
            Assert.AreEqual(new BitMask(new uint[] { 0x1E01 },33),unum3.FractionWithHiddenBit());
        }


        [Test]
        public void AddExactUnumsIsCorrect()
        {
            //example1 from book p 117
            var bitmask1 = new BitMask(new uint[] { 0xE40 }, 33);
            var bitmask2 = new BitMask(new uint[] { 0x3F22 }, 33);
            var bitmask3 = new BitMask(new uint[] { 0x7E012B }, 33);
            var unum1 = new Unum(bitmask1, 3, 4);
            var unum2 = new Unum(bitmask2, 3, 4);
            var unum3 = Unum.AddExactUnums(unum1, unum2);
            var unum_res2 = Unum.AddExactUnums(unum2, unum1);

            Assert.AreEqual(unum3.UnumBits, bitmask3);
            Assert.AreEqual(unum3.UnumBits, unum_res2.UnumBits);
            // case of inexact result, example2 from book p117
            var bitmask4 = new BitMask(new uint[] { 0x18F400CF }, 33);
            var bitmask5 = new BitMask(new uint[] { 0xE40 }, 33);
            var unum4 = 1000;
            var unum5 = new Unum(bitmask5, 3, 4);
            var unum6 = Unum.AddExactUnums(unum4, unum5);
            Assert.AreEqual(unum6.UnumBits, bitmask4);
            Unum zero = 0;
            var unum7 = Unum.AddExactUnums(zero, unum5);
            Assert.AreEqual(unum5, unum7);
        }

        [Test]
        public void SubtractExactUnumsIsCorrect()
        {
            var bitmask1 = new BitMask(new uint[] { 0x7E012B }, 33);
            var bitmask2 = new BitMask(new uint[] { 0xE40 }, 33);            
            var bitmask3 = new BitMask(new uint[] { 0x3F22 }, 33);
            
            var unum1 = new Unum(bitmask1, 3, 4);
            var unum2 = new Unum(bitmask2, 3, 4);
            var unum3 = Unum.SubtractExactUnums(unum1, unum2);
            
            Assert.AreEqual(unum3.UnumBits, bitmask3);
        }
        [Test]
        public void SetUnumBitsIsCorrect()
        {
            var unum = new Unum(3, 4);
            var Exponent = new BitMask(new uint[] { 6 }, 33);
            var Fraction = new BitMask(new uint[] { 1 }, 33);
            var WholeUnumBitMask = new BitMask(new uint[] { 0x3122 }, 33);
            unum.SetUnumBits(false, Exponent, Fraction, false, 2, 2);


            Assert.AreEqual(unum.UnumBits, WholeUnumBitMask);
        }


        [Test]
        public void IntToUnumIsCorrect()
        {
            Unum zero = 0;
            Unum thirty = 30;
            Unum thousand = 1000;
            Unum negative_thirty = -30;
            Unum negative_thousand = -1000;
            var bitmask0 = new BitMask(new uint[] { 0 }, 33);
            var bitmask30 = new BitMask(new uint[] { 0x3F22 }, 33);
            var bitmask1000 = new BitMask(new uint[] { 0x63D45 }, 33);
            var bitmask_minus_1000 = new BitMask(new uint[] { 0x63D45,1 }, 33);
            var bitmask_minus_30 = new BitMask(new uint[] { 0x3F22, 1 }, 33);

            Assert.AreEqual(zero.UnumBits, bitmask0);
            Assert.AreEqual(thirty.UnumBits, bitmask30);
            Assert.AreEqual(thousand.UnumBits, bitmask1000);
            Assert.AreEqual(negative_thirty.UnumBits, bitmask_minus_30);
        }


    }
}