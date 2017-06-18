using Hast.Common.Numerics;
using Hast.Common.Numerics.Unum;
using NUnit.Framework;


namespace Hast.Common.Tests
{
    [TestFixture]
    public class UnumTests
    {
        private UnumMetadata _metaData_3_2;
        private UnumMetadata _metaData_3_4;

        [SetUp]
        public void Init()
        {
            _metaData_3_2 = new UnumMetadata(3, 2);
            _metaData_3_4 = new UnumMetadata(3, 4);
        }

        [TestFixtureTearDown]
        public void Clean() { }

        [Test]
        public void UnumIsCorrectlyConstructedFromFloat()
        {

            var first = new Unum(_metaData_3_4, (float)30.0);
            var second = new Unum(_metaData_3_4, (float)9999999999999);


            var bitmask_1 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
            var bitmask_2 = new BitMask(new uint[] { 0x6A2309EF }, _metaData_3_4.Size);


            Assert.AreEqual(first.UnumBits, bitmask_1);
            Assert.AreEqual(second.UnumBits, bitmask_2);

        }

        [Test]
        public void UnumIsCorrectlyConstructedFromInt()
        {
            var unumZero = new Unum(_metaData_3_4, 0);
            var unumThirty = new Unum(_metaData_3_4, 30);
            var unumThousand = new Unum(_metaData_3_4, 1000);
            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
            var unumNegativeThousand = new Unum(_metaData_3_4, -1000);
            var unum6000 = new Unum(_metaData_3_4, 6000);
            var unum5000 = new Unum(_metaData_3_4, 5000);

            var bitmask0 = new BitMask(new uint[] { 0 }, _metaData_3_4.Size);
            var bitmask30 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
            var bitmask1000 = new BitMask(new uint[] { 0x63D45 }, _metaData_3_4.Size);
            var bitmaskMinus1000 = new BitMask(new uint[] { 0x63D45, 1 }, _metaData_3_4.Size);
            var bitmaskMinus30 = new BitMask(new uint[] { 0x3F22, 1 }, _metaData_3_4.Size);
            var bitmask6000 = new BitMask(new uint[] { 0x1B7747 }, _metaData_3_4.Size);
            var bitmask5000 = new BitMask(new uint[] { 0x367148 }, _metaData_3_4.Size);

            Assert.AreEqual(unumZero.UnumBits, bitmask0);
            Assert.AreEqual(unumThirty.UnumBits, bitmask30);
            Assert.AreEqual(unumThousand.UnumBits, bitmask1000);
            Assert.AreEqual(unumNegativeThirty.UnumBits, bitmaskMinus30);
            Assert.AreEqual(unum6000.UnumBits, bitmask6000);
            Assert.AreEqual(unum5000.UnumBits, bitmask5000);
            Assert.AreEqual(unumNegativeThousand.UnumBits, bitmaskMinus1000);

        }


        [Test]
        public void UnumIsCorrectlyConstructedFromUInt()
        {

            var unumZero = new Unum(_metaData_3_4, (uint)0);
            var unumThirty = new Unum(_metaData_3_4, (uint)30);
            var unumThousand = new Unum(_metaData_3_4, (uint)1000);
            var unum6000 = new Unum(_metaData_3_4, (uint)6000);
            var unum5000 = new Unum(_metaData_3_4, (uint)5000);

            var bitmask0 = new BitMask(new uint[] { 0 }, _metaData_3_4.Size);
            var bitmask30 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
            var bitmask1000 = new BitMask(new uint[] { 0x63D45 }, _metaData_3_4.Size);
            var bitmask6000 = new BitMask(new uint[] { 0x1B7747 }, _metaData_3_4.Size);
            var bitmask5000 = new BitMask(new uint[] { 0x367148 }, _metaData_3_4.Size);

            Assert.AreEqual(unumZero.UnumBits, bitmask0);
            Assert.AreEqual(unumThirty.UnumBits, bitmask30);
            Assert.AreEqual(unumThousand.UnumBits, bitmask1000);

            Assert.AreEqual(unum6000.UnumBits, bitmask6000);
            Assert.AreEqual(unum5000.UnumBits, bitmask5000);



        }
        [Test]
        public void UnumIsCorrectlyConstructedFromDouble()
        {

            var first = new Unum(_metaData_3_4, (double)30.0);
            var second = new Unum(_metaData_3_4, (double)9999999999999);


            var bitmask_1 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
            var bitmask_2 = new BitMask(new uint[] { 0x6A2309EF }, _metaData_3_4.Size);


            Assert.AreEqual(first.UnumBits, bitmask_1);
            Assert.AreEqual(second.UnumBits, bitmask_2);

        }
        //[Test]
        //public void UnumIsCorrectlyConstructedFromLong()
        //{

        //    var first = new Unum(_metaData_3_4, (long)30);
        //    var second = new Unum(_metaData_3_4, (long)9999999999999);


        //    var bitmask_1 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
        //    var bitmask_2 = new BitMask(new uint[] { 0x6A2309EF }, _metaData_3_4.Size);


        //    Assert.AreEqual(first.UnumBits, bitmask_1);
        //    Assert.AreEqual(second.UnumBits, bitmask_2);

        //}

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
            Assert.AreEqual(new BitMask(new uint[] { 0x1E01 }, 33), unum3.FractionWithHiddenBit());
        }


        [Test]
        public void AddExactUnumsIsCorrect()
        {
            //example1 from book p 117
            var bitmask1 = new BitMask(new uint[] { 0xE40 }, 33);
            var bitmask2 = new BitMask(new uint[] { 0x3F22 }, 33);
            var unum1 = new Unum(bitmask1, 3, 4);
            var unum2 = new Unum(bitmask2, 3, 4);
            var bitmask3 = new BitMask(new uint[] { 0x7E012B }, 33);
            var unum3 = Unum.AddExactUnums(unum1, unum2);
            Assert.AreEqual(unum3.UnumBits, bitmask3);

            var unum_res2 = Unum.AddExactUnums(unum2, unum1);
            Assert.AreEqual(unum3.UnumBits, unum_res2.UnumBits);

            var zero = new Unum(_metaData_3_4, 0);
            var bitmask5 = new BitMask(new uint[] { 0xE40 }, 33);
            var unum5 = new Unum(bitmask5, 3, 4);
            var unum7 = Unum.AddExactUnums(zero, unum5);
            Assert.AreEqual(unum5.UnumBits, unum7.UnumBits);


            // case of inexact result, example2 from book p117
            var bitmask4 = new BitMask(new uint[] { 0x18F400CF }, 33);
            var unum_1000 = new Unum(_metaData_3_4, 1000);
            var unum6 = Unum.AddExactUnums(unum_1000, unum5);
            Assert.AreEqual(unum6.UnumBits, bitmask4);


            var unum_5000 = new Unum(_metaData_3_4, 5000);
            var unum_6000 = new Unum(_metaData_3_4, 6000);

            Assert.AreEqual(Unum.AddExactUnums(unum_5000, unum_1000).UnumBits, unum_6000.UnumBits);

            var negative_thirty = new Unum(_metaData_3_4, -30);
            var thirty = new Unum(_metaData_3_4, 30);
            Assert.AreEqual(Unum.AddExactUnums(thirty, negative_thirty).UnumBits, zero.UnumBits);

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
            var unum5000 = new Unum(_metaData_3_4, 5000);
            var unum6000 = new Unum(_metaData_3_4, 6000);
            var unum1000 = new Unum(_metaData_3_4, 1000);

            var unumRes = Unum.SubtractExactUnums(unum6000, unum5000);
            Assert.AreEqual(unumRes.UnumBits, unum1000.UnumBits);


            Unum unumThirty = new Unum(_metaData_3_4, 30);
            Unum unumZero = new Unum(_metaData_3_4, 0);
            Assert.AreEqual(Unum.SubtractExactUnums(unumThirty, unumThirty).UnumBits, unumZero.UnumBits);


        }

        [Test]
        public void SetUnumBitsIsCorrect()
        {
            var unum = new Unum(3, 4);
            var exponent = new BitMask(new uint[] { 6 }, 33);
            var fraction = new BitMask(new uint[] { 1 }, 33);
            var wholeUnumBitMask = new BitMask(new uint[] { 0x3122 }, 33);
            unum.SetUnumBits(false, exponent, fraction, false, 2, 2);


            Assert.AreEqual(unum.UnumBits, wholeUnumBitMask);
        }

        [Test]
        public void IntToUnumIsCorrect()
        {
            var unumZero = new Unum(_metaData_3_4, 0);
            var unumThirty = new Unum(_metaData_3_4, 30);
            var unumThousand = new Unum(_metaData_3_4, 1000);
            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
            var unumNegativeThousand = new Unum(_metaData_3_4, -1000);
            var unum6000 = new Unum(_metaData_3_4, 6000);
            var unum5000 = new Unum(_metaData_3_4, 5000);

            var bitmask0 = new BitMask(new uint[] { 0 }, 33);
            var bitmask30 = new BitMask(new uint[] { 0x3F22 }, 33);
            var bitmask1000 = new BitMask(new uint[] { 0x63D45 }, 33);
            var bitmaskMinus1000 = new BitMask(new uint[] { 0x63D45, 1 }, 33);
            var bitmaskMinus30 = new BitMask(new uint[] { 0x3F22, 1 }, 33);
            var bitmask6000 = new BitMask(new uint[] { 0x1B7747 }, 33);
            var bitmask5000 = new BitMask(new uint[] { 0x367148 }, 33);

            Assert.AreEqual(unumZero.UnumBits, bitmask0);
            Assert.AreEqual(unumThirty.UnumBits, bitmask30);
            Assert.AreEqual(unumThousand.UnumBits, bitmask1000);
            Assert.AreEqual(unumNegativeThirty.UnumBits, bitmaskMinus30);
            Assert.AreEqual(unum6000.UnumBits, bitmask6000);
            Assert.AreEqual(unum5000.UnumBits, bitmask5000);
            Assert.AreEqual(unumNegativeThousand.UnumBits, bitmaskMinus1000);



        }

        [Test]
        public void UnumToUintIsCorrect()
        {


            var unumThirty = new Unum(_metaData_3_4, 30);
            var unumThousand = new Unum(_metaData_3_4, 1000);
            var unum6000 = new Unum(_metaData_3_4, 6000);
            var unum5000 = new Unum(_metaData_3_4, 5000);

            var thirty = (uint)unumThirty;
            var thousand = (uint)unumThousand;
            var number5000 = (uint)unum5000;
            var number6000 = (uint)unum6000;



            Assert.AreEqual(thirty, 30);
            Assert.AreEqual(thousand, 1000);
            Assert.AreEqual(number5000, 5000);
            Assert.AreEqual(number6000, 6000);

        }


        [Test]
        public void UnumToIntIsCorrect()
        {

            
            var unumThirty = new Unum(_metaData_3_4, 30);
            var unumThousand = new Unum(_metaData_3_4, 1000);
            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
            var unumNegativeThousand = new Unum(_metaData_3_4, -1000);
            
            var tooBigUnum = new Unum(_metaData_3_4,   9999999999999);
            var tooBigNegativeUnum = new Unum(_metaData_3_4, -9999999999999);

            var thirty = (int)unumThirty;
            var thousand = (int)unumThousand;
            var negativeThirty = (int)unumNegativeThirty;
            var negativeThousand = (int)unumNegativeThousand;
            var tooBig = (int)tooBigUnum;
            var tooBigNegative = (int)tooBigNegativeUnum;

            Assert.AreEqual(thirty, 30);
            Assert.AreEqual(thousand, 1000);
            Assert.AreEqual(negativeThirty, -30);
            Assert.AreEqual(negativeThousand, -1000);
            Assert.AreEqual(tooBig, int.MaxValue);
            Assert.AreEqual(tooBigNegative, int.MinValue);

        }

        [Test]
        public void FloatToUnumIsCorrect()
        {
            var first = new Unum(_metaData_3_4, (float)30.0);
            var second = new Unum(_metaData_3_4, (float)9999999999999);


            var bitmask_1 = new BitMask(new uint[] { 0x3F22 }, 33);
            var bitmask_2 = new BitMask(new uint[] { 0x6A2309EF }, 33);


            Assert.AreEqual(first.UnumBits, bitmask_1);
            Assert.AreEqual(second.UnumBits, bitmask_2);


        }
        [Test]
        public void UnumToFloatIsCorrect()
        {


            var unumThirty = new Unum(_metaData_3_4, 30);
            var unumThousand = new Unum(_metaData_3_4, 1000);
            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
            var unumNegativeThousand = new Unum(_metaData_3_4, -1000);

            var tooBigUnum = new Unum(_metaData_3_4,(float) 9999999999999);
            var tooBigNegativeUnum = new Unum(_metaData_3_4, (float)-9999999999999);

            var thirty = (float)unumThirty;
            var thousand = (float)unumThousand;
            var negativeThirty = (float)unumNegativeThirty;
            var negativeThousand = (float)unumNegativeThousand;
            var tooBig = (float)tooBigUnum;
            var tooBigNegative = (float)tooBigNegativeUnum;




            Assert.AreEqual(thirty, (float)30);
            Assert.AreEqual(thousand, 1000);
            Assert.AreEqual(negativeThirty, -30);
            Assert.AreEqual(negativeThousand, -1000);
            Assert.AreEqual(tooBig, (float)9999891824640); // Some information is lost due to limited size of Unum.
            Assert.AreEqual(tooBigNegative, (float)-9999891824640); // Some information is lost due to limited size of Unum.




        }

        [Test]
        public void UnumToDoubleIsCorrect()
        {

            var unumThirty = new Unum(_metaData_3_4, 30);
            var unumThousand = new Unum(_metaData_3_4, 1000);
            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
            var unumNegativeThousand = new Unum(_metaData_3_4, -1000);

            var tooBigUnum = new Unum(_metaData_3_4, (double)9999999999999);
            var tooBigNegativeUnum = new Unum(_metaData_3_4, (double)-9999999999999);

            var thirty = (double)unumThirty;
            var thousand = (double)unumThousand;
            var negativeThirty = (double)unumNegativeThirty;
            var negativeThousand = (double)unumNegativeThousand;
            var tooBig = (double)tooBigUnum;
            var tooBigNegative = (double)tooBigNegativeUnum;


            Assert.AreEqual(thirty, (double)30);
            Assert.AreEqual(thousand, (double)1000);
            Assert.AreEqual(negativeThirty, (double)-30);
            Assert.AreEqual(negativeThousand, (double)-1000);
            Assert.AreEqual(tooBig, (double)9999891824640); // Some information is lost due to limited size of Unum.
            Assert.AreEqual(tooBigNegative, (double)-9999891824640); // Some information is lost due to limited size of Unum.


        }

    }
}