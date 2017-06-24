//using NUnit.Framework;

//namespace Lombiq.Unum.Tests
//{
//    [TestFixture]
//    public class UnumTests
//    {
//        private UnumMetadata _metaData_2_2;
//        private UnumMetadata _metaData_3_4;
//        private UnumMetadata _metaData_3_5;
//        private UnumMetadata _metaData_4_8;

//        [SetUp]
//        public void Init()
//        {
//            _metaData_2_2= new UnumMetadata(2, 2);
//            _metaData_3_4 = new UnumMetadata(3, 4);
//            _metaData_3_5 = new UnumMetadata(3, 5);
//            _metaData_4_8 = new UnumMetadata(4, 8);
//        }

//        [TestFixtureTearDown]
//        public void Clean() { }

//        [Test]
//        public void UnumIsCorrectlyConstructedFromUintArray()
//        {
//            var maxValue = new uint[8];
//            for (int i = 0; i < 8; i++) maxValue[i] = uint.MaxValue;
//            maxValue[7] >>= 1;

//            var minValue = new uint[8];
//            for (int i = 0; i < 8; i++) minValue[i] = uint.MaxValue;

//            var maxUnum = new Unum(_metaData_4_8, maxValue);
//            var minUnum = new Unum(_metaData_4_8, minValue);  // This is negative.
//            var unum500000 = new Unum(_metaData_4_8, new uint[] { 500000 }); //0xC7A1250C9 
//            var unum10 = new Unum(_metaData_2_2, new uint[] { 10 });
//            var unumBig = new Unum(_metaData_4_8, new uint[] { 594967295 });
//            var zero = new Unum(_metaData_4_8, new uint[] { 0 });

//            var bitmask500000 = new BitMask(new uint[] { 0xC7A1250C }, _metaData_4_8.Size);
//            var bitmaskBig = new BitMask(new uint[] { 0xCF5FE51C, 0xF06E }, _metaData_4_8.Size); 
//            var bitmask10 = new BitMask(new uint[] { 0x329,0,0,0,0,0,0,0,0 }, _metaData_2_2.Size);

//            var bitmaskMaxValue = new BitMask(new uint[]
//            {
//                0xFFFFE8FD , 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
//                0xFFFFFFFF , 0xFFFFFFFF,  0xFEFFF

//            }, _metaData_4_8.Size);

//            var bitmaskMinValue = new BitMask(new uint[]
//            {
//                0xFFFFE8FD , 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
//                0xFFFFFFFF , 0xFFFFFFFF,  0x800FEFFF

//            }, _metaData_4_8.Size);

//            Assert.AreEqual(unum500000.UnumBits, bitmask500000);
//          //  Assert.AreEqual(unum10.UnumBits, bitmask10);

//            Assert.AreEqual(unum10.UnumBits, bitmask10);
//            Assert.AreEqual(maxUnum.IsPositive(), true);
//            Assert.AreEqual(maxUnum.Size, _metaData_4_8.Size);
//            Assert.AreEqual(maxUnum.FractionSizeWithHiddenBit(), 255);
//            Assert.AreEqual(maxUnum.ExponentValueWithBias(), 254);
//            Assert.AreEqual(maxUnum.FractionWithHiddenBit(), new BitMask(maxValue, _metaData_4_8.Size));
//            Assert.AreEqual(zero.IsZero(), true);
//            Assert.AreEqual(maxUnum.UnumBits, bitmaskMaxValue);
//            Assert.AreEqual(maxUnum.UnumBits, bitmaskMaxValue);
//        }

//        [Test]
//        public void FractionToUintArrayIsCorrect()
//        {
//            var maxValue = new uint[8];
//            for (int i = 0; i < 8; i++)
//            {
//                maxValue[i] = uint.MaxValue;
//            }
//            maxValue[7] >>= 1;

//            var maxUnum = new Unum(_metaData_4_8, maxValue);
//            var unum500000 = new Unum(_metaData_4_8, new uint[] { 500000 }); //0xC7A1250C
//            var unumBig = new Unum(_metaData_4_8, new uint[] { 594967295 });
//            var unumOne = new Unum(_metaData_4_8, new uint[] { 1 });
//            var unumZero = new Unum(_metaData_4_8, new uint[] { 0 });

//            Assert.AreEqual(unumZero.FractionToUintArray(), new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });
//            Assert.AreEqual(unumOne.FractionToUintArray(), new uint[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 });
//            Assert.AreEqual(unum500000.FractionToUintArray(), new uint[] { 500000, 0, 0, 0, 0, 0, 0, 0, 0 });
//            Assert.AreEqual(unumBig.FractionToUintArray(), new uint[] { 594967295, 0, 0, 0, 0, 0, 0, 0, 0 });
//            Assert.AreEqual(maxUnum.FractionToUintArray(), new uint[]
//                { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0x7FFFFFFF, 0 });
//        }

//        //[Test]
//        //public void UnumIsCorrectlyConstructedFromFloat()
//        //{
//        //    var first = new Unum(_metaData_3_4, (float)30.0);
//        //    var second = new Unum(_metaData_3_4, (float)9999999999999);
//        //    var third = new Unum(_metaData_3_4, (float)1.5);
//        //    var five = new Unum(_metaData_3_5, (float)5);
//        //    var fourth = new Unum(_metaData_3_5, 0.75F);


//        //    var bitmask_1 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
//        //    var bitmask_2 = new BitMask(new uint[] { 0x6A2309EF }, _metaData_3_4.Size);
//        //    var bitmask_5 = new BitMask(new uint[] { 0x1A21 }, _metaData_3_5.Size);
//        //    var bitmask_3 = new BitMask(new uint[] { 0x660 }, _metaData_3_4.Size);
//        //    var bitmask_4 = new BitMask(new uint[] { 0xA40 }, _metaData_3_5.Size);

//        //    Assert.AreEqual(first.UnumBits, bitmask_1);
//        //    Assert.AreEqual(second.UnumBits, bitmask_2);
//        //    Assert.AreEqual(five.UnumBits, bitmask_5);
//        //    Assert.AreEqual(fourth.UnumBits, bitmask_4);
//        //    Assert.AreEqual(third.UnumBits, bitmask_3);

//        //}

//        [Test]
//        public void UnumIsCorrectlyConstructedFromInt()
//        {
//            var unumZero = new Unum(_metaData_3_4, 0);
//            var unumOne = new Unum(_metaData_3_4, 1);
//            var unumThirty = new Unum(_metaData_3_4, 30);
//            var unumThousand = new Unum(_metaData_3_4, 1000);
//            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
//            var unumNegativeThousand = new Unum(_metaData_3_4, -1000);
//            var unum6000 = new Unum(_metaData_3_4, 6000);
//            var unum5000 = new Unum(_metaData_3_4, 5000);

//            var bitmask0 = new BitMask(new uint[] { 0 }, _metaData_3_4.Size);
//            var bitmask30 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
//            var bitmask1000 = new BitMask(new uint[] { 0x63D45 }, _metaData_3_4.Size);
//            var bitmaskMinus1000 = new BitMask(new uint[] { 0x63D45, 1 }, _metaData_3_4.Size);
//            var bitmaskMinus30 = new BitMask(new uint[] { 0x3F22, 1 }, _metaData_3_4.Size);
//            var bitmask6000 = new BitMask(new uint[] { 0x1B7747 }, _metaData_3_4.Size);
//            var bitmask5000 = new BitMask(new uint[] { 0x367148 }, _metaData_3_4.Size);
//            var bitmask1 = new BitMask(new uint[] { 0x100 }, _metaData_3_4.Size);

//            Assert.AreEqual(unumZero.UnumBits, bitmask0);
//            Assert.AreEqual(unumThirty.UnumBits, bitmask30);
//            Assert.AreEqual(unumThousand.UnumBits, bitmask1000);
//            Assert.AreEqual(unumNegativeThirty.UnumBits, bitmaskMinus30);
//            Assert.AreEqual(unum6000.UnumBits, bitmask6000);
//            Assert.AreEqual(unum5000.UnumBits, bitmask5000);
//            Assert.AreEqual(unumNegativeThousand.UnumBits, bitmaskMinus1000);
//            Assert.AreNotEqual(unumOne.IsZero(), true);
//            Assert.AreEqual(unumOne.UnumBits, bitmask1);
//        }

//        [Test]
//        public void UnumIsCorrectlyConstructedFromUInt()
//        {
//            var unumZero = new Unum(_metaData_3_4, (uint)0);
//            var unumOne = new Unum(_metaData_3_4, (uint)1);
//            var unumThirty = new Unum(_metaData_3_4, (uint)30);
//            var unumThousand = new Unum(_metaData_3_4, (uint)1000);
//            var unum6000 = new Unum(_metaData_3_4, (uint)6000);
//            var unum5000 = new Unum(_metaData_3_4, (uint)5000);
//            var unum10 = new Unum(_metaData_2_2, (uint) 10);

//            var bitmask10 = new BitMask(new uint[] { 0x329, 0, 0, 0, 0, 0, 0, 0, 0 }, _metaData_2_2.Size);
//            var bitmask0 = new BitMask(new uint[] { 0 }, _metaData_3_4.Size);
//            var bitmask1 = new BitMask(new uint[] { 0x100 }, _metaData_3_4.Size);
//            var bitmask30 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
//            var bitmask1000 = new BitMask(new uint[] { 0x63D45 }, _metaData_3_4.Size);
//            var bitmask6000 = new BitMask(new uint[] { 0x1B7747 }, _metaData_3_4.Size);
//            var bitmask5000 = new BitMask(new uint[] { 0x367148 }, _metaData_3_4.Size);

//            Assert.AreEqual(unum10.UnumBits, bitmask10);
//            Assert.AreEqual(unumOne.UnumBits, bitmask1);
//            Assert.AreEqual(unumZero.UnumBits, bitmask0);
//            Assert.AreEqual(unumThirty.UnumBits, bitmask30);
//            Assert.AreEqual(unumThousand.UnumBits, bitmask1000);
//            Assert.AreEqual(unum6000.UnumBits, bitmask6000);
//            Assert.AreEqual(unum5000.UnumBits, bitmask5000);
//        }

//        //[Test]
//        //public void UnumIsCorrectlyConstructedFromDouble()
//        //{
//        //    var first = new Unum(_metaData_3_4, (double)30.0);
//        //    var second = new Unum(_metaData_3_4, (double)9999999999999);


//        //    var bitmask_1 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
//        //    var bitmask_2 = new BitMask(new uint[] { 0x6A2309EF }, _metaData_3_4.Size);


//        //    Assert.AreEqual(first.UnumBits, bitmask_1);
//        //    Assert.AreEqual(second.UnumBits, bitmask_2);
//        //}


//        //[Test]
//        //public void UnumIsCorrectlyConstructedFromLong()
//        //{
//        //    var first = new Unum(_metaData_3_4, (long)30);
//        //    var second = new Unum(_metaData_3_4, (long)9999999999999);


//        //    var bitmask_1 = new BitMask(new uint[] { 0x3F22 }, _metaData_3_4.Size);
//        //    var bitmask_2 = new BitMask(new uint[] { 0x6A2309EF }, _metaData_3_4.Size);


//        //    Assert.AreEqual(first.UnumBits, bitmask_1);
//        //    Assert.AreEqual(second.UnumBits, bitmask_2);
//        //}

//        [Test]
//        public void IsExactIsCorrect()
//        {
//            // 0  0000 0000  0000  1 000 00
//            var bitmask_3_2_uncertain = new BitMask(new uint[] { 0x20 }, 19);
//            var unum_3_2_uncertain = new Unum(bitmask_3_2_uncertain, 3, 2);
//            Assert.AreEqual(unum_3_2_uncertain.IsExact(), false);

//            var bitmask_3_2_certain = new BitMask(19, false);
//            var unum_3_2_certain = new Unum(bitmask_3_2_certain, 3, 2);
//            Assert.AreEqual(unum_3_2_certain.IsExact(), true);

//            var bitmask_3_4_uncertain = new BitMask(new uint[] { 0x80, 0 }, 33);
//            var unum_3_4_uncertain = new Unum(bitmask_3_4_uncertain, 3, 4);
//            Assert.AreEqual(unum_3_4_uncertain.IsExact(), false);

//            var bitmask_3_4_certain = new BitMask(33, false);
//            var unum_3_4_certain = new Unum(bitmask_3_4_certain, 3, 4);
//            Assert.AreEqual(unum_3_4_certain.IsExact(), true);
//        }

//        [Test]
//        public void FractionSizeIsCorrect()
//        {
//            var bitmask_3_2_allOne = new BitMask(19, true);
//            var unum_3_2_allOne = new Unum(bitmask_3_2_allOne, 3, 2);
//            Assert.AreEqual(unum_3_2_allOne.FractionSize(), 4);

//            var bitmask_3_4_allOne = new BitMask(33, true);
//            var unum_3_4_allOne = new Unum(bitmask_3_4_allOne, 3, 4);
//            Assert.AreEqual(unum_3_4_allOne.FractionSize(), 16);
//        }

//        [Test]
//        public void ExponentSizeIsCorrect()
//        {
//            var bitmask_3_2_allOne = new BitMask(19, true);
//            var unum_3_2_allOne = new Unum(bitmask_3_2_allOne, 3, 2);
//            Assert.AreEqual(unum_3_2_allOne.ExponentSize(), 8);

//            var bitmask_3_4_allOne = new BitMask(33, true);
//            var unum_3_4_allOne = new Unum(bitmask_3_2_allOne, 3, 4);
//            Assert.AreEqual(unum_3_2_allOne.ExponentSize(), 8);


//            var bitmask_4_3_allOne = new BitMask(33, true);
//            var unum_4_3_allOne = new Unum(bitmask_4_3_allOne, 4, 3);
//            Assert.AreEqual(unum_4_3_allOne.ExponentSize(), 16);
//        }

//        [Test]
//        public void FractionMaskIsCorrect()
//        {
//            // 0  0000 0000  1111  0000 00
//            var bitmask_3_2_allOne = new BitMask(19, true);
//            var unum_3_2_allOne = new Unum(bitmask_3_2_allOne, 3, 2);
//            var bitmask_3_2_FractionMask = new BitMask(new uint[] { 0x3C0 }, 19);
//            Assert.AreEqual(unum_3_2_allOne.FractionMask(), bitmask_3_2_FractionMask);

//            // 0  0000 0000  1111 1111 1111 1111  0000 0000
//            var bitmask_3_4_allOne = new BitMask(33, true);
//            var unum_3_4_allOne = new Unum(bitmask_3_4_allOne, 3, 4);
//            var bitmask_3_4_FractionMask = new BitMask(new uint[] { 0xFFFF00 }, 33);
//            Assert.AreEqual(unum_3_4_allOne.FractionMask(), bitmask_3_4_FractionMask);
//        }

//        [Test]
//        public void ExponentMaskIsCorrect()
//        {
//            // 0  1111 1111  0000   0000 00
//            var bitmask_3_2_allOne = new BitMask(19, true);
//            var unum_3_2_allOne = new Unum(bitmask_3_2_allOne, 3, 2);
//            var bitmask_3_2_ExponentMask = new BitMask(new uint[] { 0x3FC00 }, 19);
//            Assert.AreEqual(unum_3_2_allOne.ExponentMask() == bitmask_3_2_ExponentMask, true);

//            // 0  1111 1111  0000 0000 0000 0000 0000 0000
//            var bitmask_3_4_allOne = new BitMask(33, true);
//            var unum_3_4_allOne = new Unum(bitmask_3_4_allOne, 3, 4);
//            var bitmask_3_4_ExponentMask = new BitMask(new uint[] { 0xFF000000 }, 33);
//            Assert.AreEqual(unum_3_4_allOne.ExponentMask() == bitmask_3_4_ExponentMask, true);
//        }

//        [Test]
//        public void ExponentValueWithBiasIsCorrect()
//        {
//            var bitmask1 = new BitMask(new uint[] { 0xE40 }, 33);
//            var unum1 = new Unum(bitmask1, 3, 4);
//            Assert.AreEqual(unum1.ExponentValueWithBias(), -8);

//            var unumZero = new Unum(_metaData_3_4, 0);
//            Assert.AreEqual(unumZero.ExponentValueWithBias(), 1);
//        }

//        [Test]
//        public void FractionWithHiddenBitIsCorrect()
//        {
//            var bitmask1 = new BitMask(new uint[] { 0xE40 }, 33);
//            var bitmask2 = new BitMask(new uint[] { 0x3F22 }, 33);
//            var bitmask3 = new BitMask(new uint[] { 0x7E012B }, 33);

//            var unum1 = new Unum(bitmask1, 3, 4);
//            var unum2 = new Unum(bitmask2, 3, 4);
//            var unum3 = new Unum(bitmask3, 3, 4);

//            Assert.AreEqual(new BitMask(new uint[] { 0x1E01 }, 33), unum3.FractionWithHiddenBit());
//            Assert.AreEqual(new BitMask(new uint[] { 0xF }, 33), unum2.FractionWithHiddenBit());
//            Assert.AreEqual(new BitMask(new uint[] { 2 }, 33), unum1.FractionWithHiddenBit());
//        }

//        [Test]
//        public void AddExactUnumsIsCorrect()
//        {
//            // First example from The End of Error p. 117.
//            var bitmask1 = new BitMask(new uint[] { 0xE40 }, 33);
//            var bitmask2 = new BitMask(new uint[] { 0x3F22 }, 33);
//            var unum1 = new Unum(bitmask1, 3, 4);
//            var unum2 = new Unum(bitmask2, 3, 4);
//            var bitmaskSum = new BitMask(new uint[] { 0x7E012B }, 33);
//            var unumSum1 = Unum.AddExactUnums(unum1, unum2);
//            Assert.AreEqual(unumSum1.UnumBits, bitmaskSum);

//            // Addition should be commutative.
//            var unumSum2 = Unum.AddExactUnums(unum2, unum1);
//            Assert.AreEqual(unumSum1.UnumBits, unumSum2.UnumBits);

//            var unumZero = new Unum(_metaData_3_4, 0);
//            var unumOne = new Unum(_metaData_3_4, 1);
//            var unumZeroPlusOne = unumZero + unumOne;
//            var unumThirtyOne = new Unum(_metaData_3_4, 30) + unumOne;
//            var unumZeroPlusUnum1 = Unum.AddExactUnums(unumZero, unum1);
//            Assert.AreEqual(unumThirtyOne.UnumBits, (new Unum(_metaData_3_4, 31).UnumBits));
//            Assert.AreEqual(unumOne.UnumBits, unumZeroPlusOne.UnumBits);
//            Assert.AreEqual(unum1.UnumBits, unumZeroPlusUnum1.UnumBits);

//            // Case of inexact result ,second example from The End or Error, p. 117.
//            var bitmask4 = new BitMask(new uint[] { 0x18F400CF }, 33); // 1000.0078125
//            var unum1000 = new Unum(_metaData_3_4, 1000);
//            var unum6 = Unum.AddExactUnums(unum1000, unum1); // 1/256
//            Assert.AreEqual(unum6.UnumBits, bitmask4);

//            var unum5000 = new Unum(_metaData_3_4, 5000);
//            var unum6000 = new Unum(_metaData_3_4, 6000);
//            Assert.AreEqual(Unum.AddExactUnums(unum5000, unum1000).UnumBits, unum6000.UnumBits);
//            //var unumTest = new Unum(_metaData_3_5, 30.401);
//            //var unumTest2 = new Unum(_metaData_3_5, -30.300);
//            //var res = unumTest + unumTest2;
//            //var resF = (double)res;
//            //Assert.AreEqual(resF, (30.401 - 30.300));// The result is correct, but the precision is too poor to show that.

//            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
//            var unumThirty = new Unum(_metaData_3_4, 30);
//            Assert.AreEqual(Unum.AddExactUnums(unumThirty, unumNegativeThirty).UnumBits, unumZero.UnumBits);
//        }

//        [Test]
//        public void AdditionIsCorrectForIntegers()
//        {
//            var result = new Unum(_metaData_3_5, 0);
//            var count = 100;

//            for (int i = 1; i <= count; i++) result += new Unum(_metaData_3_5, i * 1000);
//            for (int i = 1; i <= count; i++) result -= new Unum(_metaData_3_5, i * 1000);

//            Assert.AreEqual(result.UnumBits, new Unum(_metaData_3_5, 0).UnumBits);
//        }

//        //[Test]
//        //public void AdditionIsCorrectForFloats()
//        //{
//        //    var res = new Unum(_metaData_3_5, 0);
//        //    var res2 = new Unum(_metaData_3_5, 0);
//        //    var end = 3;
//        //    float facc = 0;

//        //    for (int i = 1; i <= end; i++)
//        //    {
//        //        facc += (float)(i * 0.5F);
//        //        res += new Unum(_metaData_3_5, (float)(i * 0.5F));
//        //    }
//        //    //res += new Unum(_metaData_3_5, 0);
//        //    for (int i = 1; i <= end; i++)
//        //    {

//        //        facc -= (float)(i * 0.5F);
//        //        res -= new Unum(_metaData_3_5, (float)(i * 0.5F));
//        //    }
//        //    var f = (float)res;
//        //    //Assert.AreEqual(res.IsZero(), true);
//        //    Assert.AreEqual(facc, f);
//        //    Assert.AreEqual(res.UnumBits, new Unum(_metaData_3_5, 0).UnumBits);

//        //    var res3 = new Unum(_metaData_3_5, 0.5F);
//        //    var res1 = new Unum(_metaData_3_5, (float)13.0);
//        //    var res2 = new Unum(_metaData_3_5, (float)4.0);
//        //    res3 = res1 + res2;
//        //    res3 -= res2;
//        //    res3 -= res1;
//        //    Assert.AreEqual((float)res3, 0.5F);
//        //}

//        [Test]
//        public void SubtractExactUnumsIsCorrect()
//        {
//            var bitmask1 = new BitMask(new uint[] { 0x7E012B }, 33); //30.00390625
//            var bitmask2 = new BitMask(new uint[] { 0xE40 }, 33);    // 0.00390625
//            var bitmask3 = new BitMask(new uint[] { 0x3F22 }, 33);  // 30

//            var unum1 = new Unum(bitmask1, 3, 4);
//            var unum2 = new Unum(bitmask2, 3, 4);
//            var unum3 = Unum.SubtractExactUnums(unum1, unum2);
//            Assert.AreEqual(unum3.UnumBits, bitmask3);

//            var unum5000 = new Unum(_metaData_3_4, 5000);
//            var unum6000 = new Unum(_metaData_3_4, 6000);
//            var unum1000 = new Unum(_metaData_3_4, 1000);

//            var unumRes = Unum.SubtractExactUnums(unum6000, unum5000);
//            Assert.AreEqual(unumRes.UnumBits, unum1000.UnumBits);

//            Unum unumThirty = new Unum(_metaData_3_4, 30);
//            Unum unumZero = new Unum(_metaData_3_4, 0);
//            Assert.AreEqual(Unum.SubtractExactUnums(unumThirty, unumThirty).UnumBits, unumZero.UnumBits);
//        }

//        [Test]
//        public void SetUnumBitsIsCorrect()
//        {
//            var unum = new Unum(3, 4);
//            var exponent = new BitMask(new uint[] { 6 }, 33);
//            var fraction = new BitMask(new uint[] { 1 }, 33);
//            var wholeUnumBitMask = new BitMask(new uint[] { 0x3122 }, 33);
//            unum.SetUnumBits(false, exponent, fraction, false, 2, 2);

//            Assert.AreEqual(unum.UnumBits, wholeUnumBitMask);
//        }

//        [Test]
//        public void IntToUnumIsCorrect()
//        {
//            var unumTwo = new Unum(_metaData_3_4, 2);
//            var unumOne = new Unum(_metaData_3_4, 1);
//            var unumZero = new Unum(_metaData_3_4, 0);
//            var unumThirty = new Unum(_metaData_3_4, 30);
//            var unumThousand = new Unum(_metaData_3_4, 1000);
//            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
//            var unumNegativeThousand = new Unum(_metaData_3_4, -1000);
//            var unum6000 = new Unum(_metaData_3_4, 6000);
//            var unum5000 = new Unum(_metaData_3_4, 5000);

//            var bitmask0 = new BitMask(new uint[] { 0 }, 33);
//            var bitmask30 = new BitMask(new uint[] { 0x3F22 }, 33);
//            var bitmask1000 = new BitMask(new uint[] { 0x63D45 }, 33);
//            var bitmaskMinus1000 = new BitMask(new uint[] { 0x63D45, 1 }, 33);
//            var bitmaskMinus30 = new BitMask(new uint[] { 0x3F22, 1 }, 33);
//            var bitmask6000 = new BitMask(new uint[] { 0x1B7747 }, 33);
//            var bitmask5000 = new BitMask(new uint[] { 0x367148 }, 33);

//            Assert.AreEqual(unumOne.UnumBits, new BitMask(new uint[] { 0x100 }, 33));
//            Assert.AreEqual(unumTwo.UnumBits, new BitMask(new uint[] { 0x200 }, 33));
//            Assert.AreEqual(unumZero.UnumBits, bitmask0);
//            Assert.AreEqual(unumThirty.UnumBits, bitmask30);
//            Assert.AreEqual(unumThousand.UnumBits, bitmask1000);
//            Assert.AreEqual(unumNegativeThirty.UnumBits, bitmaskMinus30);
//            Assert.AreEqual(unum6000.UnumBits, bitmask6000);
//            Assert.AreEqual(unum5000.UnumBits, bitmask5000);
//            Assert.AreEqual(unumNegativeThousand.UnumBits, bitmaskMinus1000);
//        }

//        [Test]
//        public void UnumToUintIsCorrect()
//        {
//            var unumTwo = new Unum(_metaData_3_4, 2);
//            var unumOne = new Unum(_metaData_3_4, 1);
//            var unumThirty = new Unum(_metaData_3_4, 30);
//            var unumThousand = new Unum(_metaData_3_4, 1000);
//            var unum6000 = new Unum(_metaData_3_4, 6000);
//            var unum5000 = new Unum(_metaData_3_4, 5000);

//            var thirty = (uint)unumThirty;
//            var thousand = (uint)unumThousand;
//            var number5000 = (uint)unum5000;
//            var number6000 = (uint)unum6000;
//            var one = (uint)unumOne;
//            var two = (uint)unumTwo;


//            Assert.AreEqual(two, 2);
//            Assert.AreEqual(one, 1);
//            Assert.AreEqual(thirty, 30);
//            Assert.AreEqual(thousand, 1000);
//            Assert.AreEqual(number5000, 5000);
//            Assert.AreEqual(number6000, 6000);
//        }

//        [Test]
//        public void UnumToIntIsCorrect()
//        {
//            var unumOne = new Unum(_metaData_3_4, 1);
//            var unumThirty = new Unum(_metaData_3_4, 30);
//            var unumThousand = new Unum(_metaData_3_4, 1000);
//            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
//            var unumNegativeThousand = new Unum(_metaData_3_4, -1000);

//            //var tooBigUnum = new Unum(_metaData_3_4, 9999999999999);
//            //var tooBigNegativeUnum = new Unum(_metaData_3_4, -9999999999999);

//            var thirty = (int)unumThirty;
//            var thousand = (int)unumThousand;
//            var negativeThirty = (int)unumNegativeThirty;
//            var negativeThousand = (int)unumNegativeThousand;
//            //var tooBig = (int)tooBigUnum;
//            //var tooBigNegative = (int)tooBigNegativeUnum;
//            var one = (int)unumOne;

//            Assert.AreEqual(one, 1);
//            Assert.AreEqual(thirty, 30);
//            Assert.AreEqual(thousand, 1000);
//            Assert.AreEqual(negativeThirty, -30);
//            Assert.AreEqual(negativeThousand, -1000);
//            //Assert.AreEqual(tooBig, int.MaxValue);
//            //Assert.AreEqual(tooBigNegative, int.MinValue);
//        }

//        //[Test]
//        //public void FloatToUnumIsCorrect()
//        //{
//        //    var first = new Unum(_metaData_3_4, (float)30.0);
//        //    var second = new Unum(_metaData_3_4, (float)9999999999999);


//        //    var bitmask_1 = new BitMask(new uint[] { 0x3F22 }, 33);
//        //    var bitmask_2 = new BitMask(new uint[] { 0x6A2309EF }, 33);


//        //    Assert.AreEqual(first.UnumBits, bitmask_1);
//        //    Assert.AreEqual(second.UnumBits, bitmask_2);
//        //}

//        [Test]
//        public void UnumToFloatIsCorrect()
//        {
//            var unumThirty = new Unum(_metaData_3_4, 30);
//            var unumThousand = new Unum(_metaData_3_4, 1000);
//            var unumNegativeThirty = new Unum(_metaData_3_4, -30);
//            var unumNegativeThousand = new Unum(_metaData_3_4, -1000);
//            //var tooBigUnum = new Unum(_metaData_3_4, (float)9999999999999);
//            //var tooBigNegativeUnum = new Unum(_metaData_3_4, (float)-9999999999999);

//            var thirty = (float)unumThirty;
//            var thousand = (float)unumThousand;
//            var negativeThirty = (float)unumNegativeThirty;
//            var negativeThousand = (float)unumNegativeThousand;
//            //var tooBig = (float)tooBigUnum;
//            //var tooBigNegative = (float)tooBigNegativeUnum;

//            Assert.AreEqual(thirty, (float)30);
//            Assert.AreEqual(thousand, 1000);
//            Assert.AreEqual(negativeThirty, -30);
//            Assert.AreEqual(negativeThousand, -1000);
//            //Assert.AreEqual(tooBig, (float)9999891824640); // Some information is lost due to limited size of Unum.
//            //Assert.AreEqual(tooBigNegative, (float)-9999891824640); // Some information is lost due to limited size of Unum.
//        }

//        //[Test]
//        //public void UnumToDoubleIsCorrect()
//        //{
//        //    var unumThirty = new Unum(_metaData_3_4, 30);
//        //    var unumThousand = new Unum(_metaData_3_4, 1000);
//        //    var unumNegativeThirty = new Unum(_metaData_3_4, -30);
//        //    var unumNegativeThousand = new Unum(_metaData_3_4, -1000);

//        //    var tooBigUnum = new Unum(_metaData_3_4, (double)9999999999999);
//        //    var tooBigNegativeUnum = new Unum(_metaData_3_4, (double)-9999999999999);

//        //    var thirty = (double)unumThirty;
//        //    var thousand = (double)unumThousand;
//        //    var negativeThirty = (double)unumNegativeThirty;
//        //    var negativeThousand = (double)unumNegativeThousand;
//        //    var tooBig = (double)tooBigUnum;
//        //    var tooBigNegative = (double)tooBigNegativeUnum;


//        //    Assert.AreEqual(thirty, (double)30);
//        //    Assert.AreEqual(thousand, (double)1000);
//        //    Assert.AreEqual(negativeThirty, (double)-30);
//        //    Assert.AreEqual(negativeThousand, (double)-1000);
//        //    Assert.AreEqual(tooBig, (double)9999891824640); // Some information is lost due to limited size of Unum.
//        //    Assert.AreEqual(tooBigNegative, (double)-9999891824640); // Some information is lost due to limited size of Unum.
//        //}

//    }
//}