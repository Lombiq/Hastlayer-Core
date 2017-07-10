using Hast.Samples.SampleAssembly.Lzma.Constants;
using Hast.Samples.SampleAssembly.Lzma.Helpers;
using Hast.Samples.SampleAssembly.Lzma.Models;
using Hast.Samples.SampleAssembly.Lzma.RangeCoder;
using Hast.Samples.SampleAssembly.Models;
//using System;

namespace Hast.Samples.SampleAssembly.Lzma
{


    public class LzmaEncoder
    {
        private const uint IfinityPrice = 0xFFFFFFF;
        private const int DefaultDictionaryLogSize = 22;
        private const uint NumFastbytesDefault = 0x20;
        private const uint NumLenSpecSymbols = BaseConstants.LowLengthSymbols + BaseConstants.MidLengthSymbols;
        private const int PropSize = 5;


        private CoderState _state = new CoderState();
        private byte _previousbyte;
        private uint[] _repDistances = new uint[BaseConstants.RepDistances];
        private Optimal[] _optimum = new Optimal[BaseConstants.OptimumNumber];
        private BinTree _matchFinder = null;
        private RangeEncoder _rangeEncoder = new RangeEncoder();
        private BitEncoder[] _isMatch = new BitEncoder[BaseConstants.States << BaseConstants.MaxPositionStatesBits];
        private BitEncoder[] _isRep = new BitEncoder[BaseConstants.States];
        private BitEncoder[] _isRepG0 = new BitEncoder[BaseConstants.States];
        private BitEncoder[] _isRepG1 = new BitEncoder[BaseConstants.States];
        private BitEncoder[] _isRepG2 = new BitEncoder[BaseConstants.States];
        private BitEncoder[] _isRep0Long = new BitEncoder[BaseConstants.States << BaseConstants.MaxPositionStatesBits];
        private BitTreeEncoder[] _posSlotEncoder = new BitTreeEncoder[BaseConstants.LengthToPositionStates];
        private BitEncoder[] _posEncoders = new BitEncoder[BaseConstants.FullDistances - BaseConstants.EndPositionModelIndex];
        private BitTreeEncoder _posAlignEncoder = new BitTreeEncoder(BaseConstants.AlignBits);
        private LenPriceTableEncoder _lenEncoder = new LenPriceTableEncoder();
        private LenPriceTableEncoder _repMatchLenEncoder = new LenPriceTableEncoder();
        private LiteralEncoder _literalEncoder = new LiteralEncoder();
        private uint[] _matchDistances = new uint[BaseConstants.MaxMatchLength * 2 + 2];
        private uint _numberOfFastBytes = NumFastbytesDefault;
        private uint _longestMatchLength;
        private uint _numDistancePairs;
        private uint _additionalOffset;
        private uint _optimumEndIndex;
        private uint _optimumCurrentIndex;
        private bool _longestMatchWasFound;
        private uint[] _posSlotPrices = new uint[1 << (BaseConstants.PositionSlotBits + BaseConstants.LengthToPositionStatesBits)];
        private uint[] _distancesPrices = new uint[BaseConstants.FullDistances << BaseConstants.LengthToPositionStatesBits];
        private uint[] _alignPrices = new uint[BaseConstants.AlignTableSize];
        private uint _alignPriceCount;
        private uint _distanceTableSize = (DefaultDictionaryLogSize * 2);
        private int _posStateBits = 2;
        private uint _posStateMask = (4 - 1);
        private int _literalPositionBits = 0;
        private int _literalContextBits = 3;
        private uint _dictionarySize = (1 << DefaultDictionaryLogSize);
        private uint _dictionarySizePrev = 0xFFFFFFFF;
        private uint _numFastbytesPrev = 0xFFFFFFFF;
        private long nowPos64;
        private bool _finished;
        private SimpleMemoryStream _inStream;
        private uint _numberOfMatchFinderHashBytes = 4;
        private bool _writeEndMark = false;
        private bool _needReleaseMFStream;
        private byte[] properties = new byte[PropSize];
        private uint[] tempPrices = new uint[BaseConstants.FullDistances];
        private uint _matchPriceCount;
        private uint _trainSize = 0;
        private uint[] reps = new uint[BaseConstants.RepDistances];
        private uint[] repLens = new uint[BaseConstants.RepDistances];
        private byte[] _fastPos = new byte[1 << 11];


        public LzmaEncoder()
        {
            const byte kFastSlots = 22;
            int c = 2;
            _fastPos[0] = 0;
            _fastPos[1] = 1;
            for (byte slotFast = 2; slotFast < kFastSlots; slotFast++)
            {
                uint k = ((uint)1 << ((slotFast >> 1) - 1));
                for (uint j = 0; j < k; j++, c++)
                    _fastPos[c] = slotFast;
            }

            for (int i = 0; i < BaseConstants.OptimumNumber; i++)
                _optimum[i] = new Optimal();
            for (int i = 0; i < BaseConstants.LengthToPositionStates; i++)
                _posSlotEncoder[i] = new BitTreeEncoder(BaseConstants.PositionSlotBits);
        }


        public bool CodeOneBlock()
        {
            long inSize = 0;
            long outSize = 0;
            var finished = true;

            if (_inStream != null)
            {
                _matchFinder.SetStream(_inStream);
                _matchFinder.Init();
                _needReleaseMFStream = true;
                _inStream = null;
                if (_trainSize > 0)
                    _matchFinder.Skip(_trainSize);
            }

            var run = true;
            if (!_finished)
            {
                _finished = true;


                long progressPosValuePrev = nowPos64;
                if (nowPos64 == 0)
                {
                    if (_matchFinder.GetNumAvailablebytes() == 0)
                    {
                        Flush((uint)nowPos64);
                        run = false;
                    }
                    else
                    {
                        uint len, numDistancePairs; // it's not used
                        ReadMatchDistances();
                        uint posState = (uint)(nowPos64) & _posStateMask;
                        _isMatch[(_state.Index << BaseConstants.MaxPositionStatesBits) + posState].Encode(_rangeEncoder, 0);
                        _state.UpdateChar();
                        byte curbyte = _matchFinder.GetIndexbyte((int)(0 - _additionalOffset));
                        _literalEncoder.GetSubCoder((uint)(nowPos64), _previousbyte).Encode(_rangeEncoder, curbyte);
                        _previousbyte = curbyte;
                        _additionalOffset--;
                        nowPos64++;
                    }
                }
                if (run)
                {
                    if (_matchFinder.GetNumAvailablebytes() == 0)
                    {
                        Flush((uint)nowPos64);
                    }
                    else
                    {
                        while (run)
                        {
                            var returnValues = GetOptimum((uint)nowPos64);

                            var pos = returnValues.OutValue;
                            var len = returnValues.ReturnValue;

                            uint posState = ((uint)nowPos64) & _posStateMask;
                            uint complexState = (_state.Index << BaseConstants.MaxPositionStatesBits) + posState;
                            if (len == 1 && pos == 0xFFFFFFFF)
                            {
                                _isMatch[complexState].Encode(_rangeEncoder, 0);
                                byte curbyte = _matchFinder.GetIndexbyte((int)(0 - _additionalOffset));
                                LiteralEncoder.Encoder2 subCoder = _literalEncoder.GetSubCoder((uint)nowPos64, _previousbyte);
                                if (!_state.IsCharState())
                                {
                                    byte matchbyte = _matchFinder.GetIndexbyte((int)(0 - _repDistances[0] - 1 - _additionalOffset));
                                    subCoder.EncodeMatched(_rangeEncoder, matchbyte, curbyte);
                                }
                                else
                                    subCoder.Encode(_rangeEncoder, curbyte);
                                _previousbyte = curbyte;
                                _state.UpdateChar();
                            }
                            else
                            {
                                _isMatch[complexState].Encode(_rangeEncoder, 1);
                                if (pos < BaseConstants.RepDistances)
                                {
                                    _isRep[_state.Index].Encode(_rangeEncoder, 1);
                                    if (pos == 0)
                                    {
                                        _isRepG0[_state.Index].Encode(_rangeEncoder, 0);
                                        if (len == 1)
                                            _isRep0Long[complexState].Encode(_rangeEncoder, 0);
                                        else
                                            _isRep0Long[complexState].Encode(_rangeEncoder, 1);
                                    }
                                    else
                                    {
                                        _isRepG0[_state.Index].Encode(_rangeEncoder, 1);
                                        if (pos == 1)
                                            _isRepG1[_state.Index].Encode(_rangeEncoder, 0);
                                        else
                                        {
                                            _isRepG1[_state.Index].Encode(_rangeEncoder, 1);
                                            _isRepG2[_state.Index].Encode(_rangeEncoder, pos - 2);
                                        }
                                    }
                                    if (len == 1)
                                        _state.UpdateShortRep();
                                    else
                                    {
                                        _repMatchLenEncoder.Encode(_rangeEncoder, len - BaseConstants.MinMatchLength, posState);
                                        _state.UpdateRep();
                                    }
                                    uint distance = _repDistances[pos];
                                    if (pos != 0)
                                    {
                                        for (uint i = pos; i >= 1; i--)
                                            _repDistances[i] = _repDistances[i - 1];
                                        _repDistances[0] = distance;
                                    }
                                }
                                else
                                {
                                    _isRep[_state.Index].Encode(_rangeEncoder, 0);
                                    _state.UpdateMatch();
                                    _lenEncoder.Encode(_rangeEncoder, len - BaseConstants.MinMatchLength, posState);
                                    pos -= BaseConstants.RepDistances;
                                    uint posSlot = GetPosSlot(pos);
                                    uint lenToPosState = BaseConstants.GetLengthToPositionState(len);
                                    _posSlotEncoder[lenToPosState].Encode(_rangeEncoder, posSlot);

                                    if (posSlot >= BaseConstants.StartPositionModelIndex)
                                    {
                                        int footerBits = (int)((posSlot >> 1) - 1);
                                        uint baseVal = ((2 | (posSlot & 1)) << footerBits);
                                        uint posReduced = pos - baseVal;

                                        if (posSlot < BaseConstants.EndPositionModelIndex)
                                            BitTreeEncoder.ReverseEncode(_posEncoders,
                                                    baseVal - posSlot - 1, _rangeEncoder, footerBits, posReduced);
                                        else
                                        {
                                            _rangeEncoder.EncodeDirectBits(posReduced >> BaseConstants.AlignBits, footerBits - BaseConstants.AlignBits);
                                            _posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & BaseConstants.AlignMask);
                                            _alignPriceCount++;
                                        }
                                    }
                                    uint distance = pos;
                                    for (uint i = BaseConstants.RepDistances - 1; i >= 1; i--)
                                        _repDistances[i] = _repDistances[i - 1];
                                    _repDistances[0] = distance;
                                    _matchPriceCount++;
                                }
                                _previousbyte = _matchFinder.GetIndexbyte((int)(len - 1 - _additionalOffset));
                            }
                            _additionalOffset -= len;
                            nowPos64 += len;
                            if (_additionalOffset == 0)
                            {
                                // if (!_fastMode)
                                if (_matchPriceCount >= (1 << 7))
                                    FillDistancesPrices();
                                if (_alignPriceCount >= BaseConstants.AlignTableSize)
                                    FillAlignPrices();
                                inSize = nowPos64;
                                outSize = _rangeEncoder.GetProcessedSizeAdd();
                                if (_matchFinder.GetNumAvailablebytes() == 0)
                                {
                                    Flush((uint)nowPos64);
                                    run = false;
                                }
                                else if (nowPos64 - progressPosValuePrev >= (1 << 12))
                                {
                                    _finished = false;
                                    finished = false;
                                    run = false;
                                }
                            }
                        }
                    }
                }
            }

            return finished;
        }

        public void Encode(SimpleMemoryStream inStream, SimpleMemoryStream outStream)
        {
            _needReleaseMFStream = false;

            SetStreams(inStream, outStream);
            var finished = false;
            while (!finished)
            {
                finished = CodeOneBlock();
            }
        }

        public void SetCoderProperties(EncoderProperties properties)
        {
            _numberOfFastBytes = properties.NumberOfFastBytes;
            
            _numberOfMatchFinderHashBytes = properties.NumberOfMatchFinderHashBytes;
            _dictionarySizePrev = 0xFFFFFFFF;
            _matchFinder = null;

            // Should throw an Exception when it becomes supported.
            // if (dictionarySize < (uint)(1 << BaseConstants.DicLogSizeMin) ||
            //     dictionarySize > (uint)(1 << kDicLogSizeMaxCompress))

            const int DictionaryLogSizeMaxCompress = 30;
            _dictionarySize = properties.DictionarySize;
            int dictionaryLogSize;
            for (dictionaryLogSize = 0; dictionaryLogSize < (uint)DictionaryLogSizeMaxCompress; dictionaryLogSize++)
                if (_dictionarySize <= ((uint)(1) << dictionaryLogSize))
                    break;
            _distanceTableSize = (uint)dictionaryLogSize * 2;

            // Should throw an Exception when it becomes supported.
            // if (params.PositionStateBits < 0 || params.PositionStateBits > (uint)BaseConstants.NumPosStatesBitsEncodingMax)
            _posStateBits = properties.PositionStateBits;
            _posStateMask = (((uint)1) << _posStateBits) - 1;

            // Should throw an Exception when it becomes supported.
            // if (parameters.LiteralPositionBits < 0 || 
            //     parameters.LiteralPositionBits > (uint)BaseConstants.NumLitPosStatesBitsEncodingMax)
            _literalPositionBits = properties.LiteralPositionBits;

            // Should throw an Exception when it becomes supported.
            // if (parameters.LiteralContextBits < 0 || parameters.LiteralContextBits > (uint)BaseConstants.NumLitContextBitsMax)
            //     throw new LzmaInvalidParamException(); ;
            _literalContextBits = properties.LiteralContextBits;

            SetWriteEndMarkerMode(properties.WriteEndMarker);

            // Algorithm parameter is not yet processed. The related code lines are commented out
            // in the original LZMA SDK too:
            /*
            int maximize = (int)prop;
            _fastMode = (maximize == 0);
            _maxMode = (maximize >= 2);
            */
        }

        public void SetTrainSize(uint trainSize)
        {
            _trainSize = trainSize;
        }

        public void WriteCoderProperties(SimpleMemoryStream outStream)
        {
            properties[0] = (byte)((_posStateBits * 5 + _literalPositionBits) * 9 + _literalContextBits);
            for (int i = 0; i < 4; i++)
                properties[1 + i] = (byte)((_dictionarySize >> (8 * i)) & 0xFF);
            outStream.Write(properties, 0, PropSize);
        }


        private uint GetPosSlot(uint pos)
        {
            if (pos < (1 << 11))
                return _fastPos[pos];
            if (pos < (1 << 21))
                return (uint)(_fastPos[pos >> 10] + 20);
            return (uint)(_fastPos[pos >> 20] + 40);
        }

        private uint GetPosSlot2(uint pos)
        {
            if (pos < (1 << 17))
                return (uint)(_fastPos[pos >> 6] + 12);
            if (pos < (1 << 27))
                return (uint)(_fastPos[pos >> 16] + 32);
            return (uint)(_fastPos[pos >> 26] + 52);
        }

        private void BaseInit()
        {
            _state.Init();
            _previousbyte = 0;
            for (uint i = 0; i < BaseConstants.RepDistances; i++)
                _repDistances[i] = 0;
        }

        private void Create()
        {
            var binTree = new BinTree();
            binTree.SetType(_numberOfMatchFinderHashBytes);
            _matchFinder = binTree;

            _literalEncoder.Create(_literalPositionBits, _literalContextBits);

            if (_dictionarySize != _dictionarySizePrev || _numFastbytesPrev != _numberOfFastBytes)
            {
                _matchFinder.Create(_dictionarySize, BaseConstants.OptimumNumber, _numberOfFastBytes, BaseConstants.MaxMatchLength + 1);
                _dictionarySizePrev = _dictionarySize;
                _numFastbytesPrev = _numberOfFastBytes;
            }
        }

        private void SetWriteEndMarkerMode(bool writeEndMarker)
        {
            _writeEndMark = writeEndMarker;
        }

        private void Init()
        {
            BaseInit();
            _rangeEncoder.Init();

            var probPrices = new uint[RangeEncoderConstants.BitModelTotal >> RangeEncoderConstants.MoveReducingBits];
            const int kNumBits = (RangeEncoderConstants.BitModelTotalBits - RangeEncoderConstants.MoveReducingBits);
            for (int k = kNumBits - 1; k >= 0; k--)
            {
                var start = (uint)1 << (kNumBits - k - 1);
                var end = (uint)1 << (kNumBits - k);
                for (var j = start; j < end; j++)
                {
                    probPrices[j] = ((uint)k << RangeEncoderConstants.BitPriceShiftBits) +
                        (((end - j) << RangeEncoderConstants.BitPriceShiftBits) >> (kNumBits - k - 1));
                }
            }

            uint i;
            for (i = 0; i < BaseConstants.States; i++)
            {
                for (uint j = 0; j <= _posStateMask; j++)
                {
                    uint complexState = (i << BaseConstants.MaxPositionStatesBits) + j;
                    _isMatch[complexState] = new BitEncoder();
                    _isMatch[complexState].Init(probPrices);
                    _isRep0Long[complexState] = new BitEncoder();
                    _isRep0Long[complexState].Init(probPrices);
                }
                _isRep[i] = new BitEncoder();
                _isRep[i].Init(probPrices);
                _isRepG0[i] = new BitEncoder();
                _isRepG0[i].Init(probPrices);
                _isRepG1[i] = new BitEncoder();
                _isRepG1[i].Init(probPrices);
                _isRepG2[i] = new BitEncoder();
                _isRepG2[i].Init(probPrices);
            }
            _literalEncoder.Init(probPrices);
            for (i = 0; i < BaseConstants.LengthToPositionStates; i++)
                _posSlotEncoder[i].Init(probPrices);
            for (i = 0; i < BaseConstants.FullDistances - BaseConstants.EndPositionModelIndex; i++)
            {
                _posEncoders[i] = new BitEncoder();
                _posEncoders[i].Init(probPrices);
            }

            _lenEncoder.InitLenEncoder((uint)1 << _posStateBits, probPrices);
            _repMatchLenEncoder.InitLenEncoder((uint)1 << _posStateBits, probPrices);

            _posAlignEncoder.Init(probPrices);

            _longestMatchWasFound = false;
            _optimumEndIndex = 0;
            _optimumCurrentIndex = 0;
            _additionalOffset = 0;
        }

        private OutResult ReadMatchDistances()
        {
            uint lenRes = 0;
            uint numDistancePairs = _matchFinder.GetMatches(_matchDistances);
            if (numDistancePairs > 0)
            {
                lenRes = _matchDistances[numDistancePairs - 2];
                if (lenRes == _numberOfFastBytes)
                    lenRes += _matchFinder.GetMatchLen((int)lenRes - 1, _matchDistances[numDistancePairs - 1],
                        BaseConstants.MaxMatchLength - lenRes);
            }
            _additionalOffset++;

            return new OutResult { ReturnValue = lenRes, OutValue = numDistancePairs };
        }

        private void MovePos(uint num)
        {
            if (num > 0)
            {
                _matchFinder.Skip(num);
                _additionalOffset += num;
            }
        }

        private uint GetRepLen1Price(CoderState state, uint posState)
        {
            return _isRepG0[state.Index].GetPrice0() +
                    _isRep0Long[(state.Index << BaseConstants.MaxPositionStatesBits) + posState].GetPrice0();
        }

        private uint GetPureRepPrice(uint repIndex, CoderState state, uint posState)
        {
            uint price;
            if (repIndex == 0)
            {
                price = _isRepG0[state.Index].GetPrice0();
                price += _isRep0Long[(state.Index << BaseConstants.MaxPositionStatesBits) + posState].GetPrice1();
            }
            else
            {
                price = _isRepG0[state.Index].GetPrice1();
                if (repIndex == 1)
                    price += _isRepG1[state.Index].GetPrice0();
                else
                {
                    price += _isRepG1[state.Index].GetPrice1();
                    price += _isRepG2[state.Index].GetPrice(repIndex - 2);
                }
            }
            return price;
        }

        private uint GetRepPrice(uint repIndex, uint len, CoderState state, uint posState)
        {
            uint price = _repMatchLenEncoder.GetPrice(len - BaseConstants.MinMatchLength, posState);
            return price + GetPureRepPrice(repIndex, state, posState);
        }

        private uint GetPosLenPrice(uint pos, uint len, uint posState)
        {
            uint price;
            uint lenToPosState = BaseConstants.GetLengthToPositionState(len);
            if (pos < BaseConstants.FullDistances)
                price = _distancesPrices[(lenToPosState * BaseConstants.FullDistances) + pos];
            else
                price = _posSlotPrices[(lenToPosState << BaseConstants.PositionSlotBits) + GetPosSlot2(pos)] +
                    _alignPrices[pos & BaseConstants.AlignMask];
            return price + _lenEncoder.GetPrice(len - BaseConstants.MinMatchLength, posState);
        }

        private OutResult Backward(uint cur)
        {
            // ReturnValue: _optimumCurrentIndex, OutValue: backRes
            var returnValues = new OutResult { ReturnValue = 0, OutValue = 0 };

            _optimumEndIndex = cur;
            uint posMem = _optimum[cur].PosPrev;
            uint backMem = _optimum[cur].BackPrev;
            do
            {
                if (_optimum[cur].Prev1IsChar)
                {
                    _optimum[posMem].MakeAsChar();
                    _optimum[posMem].PosPrev = posMem - 1;
                    if (_optimum[cur].Prev2)
                    {
                        _optimum[posMem - 1].Prev1IsChar = false;
                        _optimum[posMem - 1].PosPrev = _optimum[cur].PosPrev2;
                        _optimum[posMem - 1].BackPrev = _optimum[cur].BackPrev2;
                    }
                }
                uint posPrev = posMem;
                uint backCur = backMem;

                backMem = _optimum[posPrev].BackPrev;
                posMem = _optimum[posPrev].PosPrev;

                _optimum[posPrev].BackPrev = backCur;
                _optimum[posPrev].PosPrev = cur;
                cur = posPrev;
            }
            while (cur > 0);
            returnValues.OutValue = _optimum[0].BackPrev;
            _optimumCurrentIndex = _optimum[0].PosPrev;

            returnValues.ReturnValue = _optimumCurrentIndex;

            return returnValues;
        }

        private OutResult GetOptimum(uint position)
        {
            // ReturnValue: lenRes, OutValue: backRes
            var returnValues = new OutResult { ReturnValue = 0, OutValue = 0 };

            if (_optimumEndIndex != _optimumCurrentIndex)
            {
                returnValues.ReturnValue = _optimum[_optimumCurrentIndex].PosPrev - _optimumCurrentIndex;
                returnValues.OutValue = _optimum[_optimumCurrentIndex].BackPrev;
                _optimumCurrentIndex = _optimum[_optimumCurrentIndex].PosPrev;
                // Return.
            }
            else
            {
                _optimumCurrentIndex = _optimumEndIndex = 0;

                uint lenMain, numDistancePairs;
                if (!_longestMatchWasFound)
                {
                    var result = ReadMatchDistances();

                    lenMain = result.ReturnValue;
                    numDistancePairs = result.OutValue;
                }
                else
                {
                    lenMain = _longestMatchLength;
                    numDistancePairs = _numDistancePairs;
                    _longestMatchWasFound = false;
                }

                uint numAvailablebytes = _matchFinder.GetNumAvailablebytes() + 1;
                if (numAvailablebytes < 2)
                {
                    returnValues.OutValue = 0xFFFFFFFF;
                    // Return.
                    returnValues.ReturnValue = 1;
                }
                else
                {
                    if (numAvailablebytes > BaseConstants.MaxMatchLength)
                        numAvailablebytes = BaseConstants.MaxMatchLength;

                    uint repMaxIndex = 0;
                    uint i;
                    for (i = 0; i < BaseConstants.RepDistances; i++)
                    {
                        reps[i] = _repDistances[i];
                        repLens[i] = _matchFinder.GetMatchLen(0 - 1, reps[i], BaseConstants.MaxMatchLength);
                        if (repLens[i] > repLens[repMaxIndex])
                            repMaxIndex = i;
                    }
                    if (repLens[repMaxIndex] >= _numberOfFastBytes)
                    {
                        returnValues.OutValue = repMaxIndex;
                        returnValues.ReturnValue = repLens[repMaxIndex];
                        MovePos(returnValues.ReturnValue - 1);

                        // Return;
                    }
                    else
                    {
                        if (lenMain >= _numberOfFastBytes)
                        {
                            returnValues.OutValue = _matchDistances[numDistancePairs - 1] + BaseConstants.RepDistances;
                            MovePos(lenMain - 1);

                            // Return.
                            returnValues.ReturnValue = lenMain;
                        }
                        else
                        {

                            byte currentbyte = _matchFinder.GetIndexbyte(0 - 1);
                            byte matchbyte = _matchFinder.GetIndexbyte((int)(0 - _repDistances[0] - 1 - 1));

                            if (lenMain < 2 && currentbyte != matchbyte && repLens[repMaxIndex] < 2)
                            {
                                returnValues.OutValue = (uint)0xFFFFFFFF;

                                returnValues.ReturnValue = 1;
                                // Return.
                            }
                            else
                            {
                                _optimum[0].State = _state;

                                uint posState = (position & _posStateMask);

                                _optimum[1].Price = _isMatch[(_state.Index << BaseConstants.MaxPositionStatesBits) + posState].GetPrice0() +
                                        _literalEncoder.GetSubCoder(position, _previousbyte).GetPrice(!_state.IsCharState(), matchbyte, currentbyte);
                                _optimum[1].MakeAsChar();

                                uint matchPrice = _isMatch[(_state.Index << BaseConstants.MaxPositionStatesBits) + posState].GetPrice1();
                                uint repMatchPrice = matchPrice + _isRep[_state.Index].GetPrice1();

                                if (matchbyte == currentbyte)
                                {
                                    uint shortRepPrice = repMatchPrice + GetRepLen1Price(_state, posState);
                                    if (shortRepPrice < _optimum[1].Price)
                                    {
                                        _optimum[1].Price = shortRepPrice;
                                        _optimum[1].MakeAsShortRep();
                                    }
                                }

                                uint lenEnd = ((lenMain >= repLens[repMaxIndex]) ? lenMain : repLens[repMaxIndex]);

                                if (lenEnd < 2)
                                {
                                    returnValues.OutValue = _optimum[1].BackPrev;
                                    returnValues.ReturnValue = 1;

                                    // Return.
                                }
                                else
                                {

                                    _optimum[1].PosPrev = 0;

                                    _optimum[0].Backs0 = reps[0];
                                    _optimum[0].Backs1 = reps[1];
                                    _optimum[0].Backs2 = reps[2];
                                    _optimum[0].Backs3 = reps[3];

                                    uint len = lenEnd;
                                    do
                                        _optimum[len--].Price = IfinityPrice;
                                    while (len >= 2);

                                    for (i = 0; i < BaseConstants.RepDistances; i++)
                                    {
                                        uint repLen = repLens[i];
                                        if (repLen >= 2)
                                        {
                                            uint price = repMatchPrice + GetPureRepPrice(i, _state, posState);
                                            do
                                            {
                                                uint curAndLenPrice = price + _repMatchLenEncoder.GetPrice(repLen - 2, posState);
                                                Optimal optimum = _optimum[repLen];
                                                if (curAndLenPrice < optimum.Price)
                                                {
                                                    optimum.Price = curAndLenPrice;
                                                    optimum.PosPrev = 0;
                                                    optimum.BackPrev = i;
                                                    optimum.Prev1IsChar = false;
                                                }
                                            }
                                            while (--repLen >= 2);
                                        }
                                    }

                                    uint normalMatchPrice = matchPrice + _isRep[_state.Index].GetPrice0();

                                    len = ((repLens[0] >= 2) ? repLens[0] + 1 : 2);
                                    if (len <= lenMain)
                                    {
                                        uint offs = 0;
                                        while (len > _matchDistances[offs])
                                            offs += 2;
                                        var run2 = true;
                                        while (run2)
                                        {
                                            uint distance = _matchDistances[offs + 1];
                                            uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(distance, len, posState);
                                            Optimal optimum = _optimum[len];
                                            if (curAndLenPrice < optimum.Price)
                                            {
                                                optimum.Price = curAndLenPrice;
                                                optimum.PosPrev = 0;
                                                optimum.BackPrev = distance + BaseConstants.RepDistances;
                                                optimum.Prev1IsChar = false;
                                            }

                                            if (len == _matchDistances[offs])
                                            {
                                                offs += 2;
                                                if (offs == numDistancePairs)
                                                    // Break.
                                                    run2 = false;
                                            }
                                            // It's because we are possibly in the break.
                                            if (run2) len++;
                                        }
                                    }

                                    uint cur = 0;

                                    var run = true;
                                    while (run)
                                    {
                                        cur++;
                                        if (cur == lenEnd)
                                        {
                                            var backwardReturnValues = Backward(cur);
                                            returnValues.ReturnValue = backwardReturnValues.ReturnValue;
                                            returnValues.OutValue = backwardReturnValues.OutValue;
                                            run = false;
                                            // Return.
                                        }
                                        else
                                        {
                                            uint newLen;
                                            var result = ReadMatchDistances();
                                            newLen = result.ReturnValue;
                                            numDistancePairs = result.OutValue;
                                            if (newLen >= _numberOfFastBytes)
                                            {
                                                _numDistancePairs = numDistancePairs;
                                                _longestMatchLength = newLen;
                                                _longestMatchWasFound = true;
                                                var backwardReturnValues = Backward(cur);
                                                returnValues.ReturnValue = backwardReturnValues.ReturnValue;
                                                returnValues.OutValue = backwardReturnValues.OutValue;
                                                run = false;

                                                // Return.
                                            }
                                            else
                                            {
                                                position++;
                                                uint posPrev = _optimum[cur].PosPrev;
                                                CoderState state;
                                                if (_optimum[cur].Prev1IsChar)
                                                {
                                                    posPrev--;
                                                    if (_optimum[cur].Prev2)
                                                    {
                                                        state = _optimum[_optimum[cur].PosPrev2].State;
                                                        if (_optimum[cur].BackPrev2 < BaseConstants.RepDistances)
                                                            state.UpdateRep();
                                                        else
                                                            state.UpdateMatch();
                                                    }
                                                    else
                                                        state = _optimum[posPrev].State;
                                                    state.UpdateChar();
                                                }
                                                else
                                                    state = _optimum[posPrev].State;
                                                if (posPrev == cur - 1)
                                                {
                                                    if (_optimum[cur].IsShortRep())
                                                        state.UpdateShortRep();
                                                    else
                                                        state.UpdateChar();
                                                }
                                                else
                                                {
                                                    uint pos;
                                                    if (_optimum[cur].Prev1IsChar && _optimum[cur].Prev2)
                                                    {
                                                        posPrev = _optimum[cur].PosPrev2;
                                                        pos = _optimum[cur].BackPrev2;
                                                        state.UpdateRep();
                                                    }
                                                    else
                                                    {
                                                        pos = _optimum[cur].BackPrev;
                                                        if (pos < BaseConstants.RepDistances)
                                                            state.UpdateRep();
                                                        else
                                                            state.UpdateMatch();
                                                    }
                                                    Optimal opt = _optimum[posPrev];
                                                    if (pos < BaseConstants.RepDistances)
                                                    {
                                                        if (pos == 0)
                                                        {
                                                            reps[0] = opt.Backs0;
                                                            reps[1] = opt.Backs1;
                                                            reps[2] = opt.Backs2;
                                                            reps[3] = opt.Backs3;
                                                        }
                                                        else if (pos == 1)
                                                        {
                                                            reps[0] = opt.Backs1;
                                                            reps[1] = opt.Backs0;
                                                            reps[2] = opt.Backs2;
                                                            reps[3] = opt.Backs3;
                                                        }
                                                        else if (pos == 2)
                                                        {
                                                            reps[0] = opt.Backs2;
                                                            reps[1] = opt.Backs0;
                                                            reps[2] = opt.Backs1;
                                                            reps[3] = opt.Backs3;
                                                        }
                                                        else
                                                        {
                                                            reps[0] = opt.Backs3;
                                                            reps[1] = opt.Backs0;
                                                            reps[2] = opt.Backs1;
                                                            reps[3] = opt.Backs2;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        reps[0] = (pos - BaseConstants.RepDistances);
                                                        reps[1] = opt.Backs0;
                                                        reps[2] = opt.Backs1;
                                                        reps[3] = opt.Backs2;
                                                    }
                                                }
                                                _optimum[cur].State = state;
                                                _optimum[cur].Backs0 = reps[0];
                                                _optimum[cur].Backs1 = reps[1];
                                                _optimum[cur].Backs2 = reps[2];
                                                _optimum[cur].Backs3 = reps[3];
                                                uint curPrice = _optimum[cur].Price;

                                                currentbyte = _matchFinder.GetIndexbyte(0 - 1);
                                                matchbyte = _matchFinder.GetIndexbyte((int)(0 - reps[0] - 1 - 1));

                                                posState = (position & _posStateMask);

                                                uint curAnd1Price = curPrice +
                                                    _isMatch[(state.Index << BaseConstants.MaxPositionStatesBits) + posState].GetPrice0() +
                                                    _literalEncoder.GetSubCoder(position, _matchFinder.GetIndexbyte(0 - 2)).
                                                    GetPrice(!state.IsCharState(), matchbyte, currentbyte);

                                                Optimal nextOptimum = _optimum[cur + 1];

                                                bool nextIsChar = false;
                                                if (curAnd1Price < nextOptimum.Price)
                                                {
                                                    nextOptimum.Price = curAnd1Price;
                                                    nextOptimum.PosPrev = cur;
                                                    nextOptimum.MakeAsChar();
                                                    nextIsChar = true;
                                                }

                                                matchPrice = curPrice + _isMatch[(state.Index << BaseConstants.MaxPositionStatesBits) + posState].GetPrice1();
                                                repMatchPrice = matchPrice + _isRep[state.Index].GetPrice1();

                                                if (matchbyte == currentbyte &&
                                                    !(nextOptimum.PosPrev < cur && nextOptimum.BackPrev == 0))
                                                {
                                                    uint shortRepPrice = repMatchPrice + GetRepLen1Price(state, posState);
                                                    if (shortRepPrice <= nextOptimum.Price)
                                                    {
                                                        nextOptimum.Price = shortRepPrice;
                                                        nextOptimum.PosPrev = cur;
                                                        nextOptimum.MakeAsShortRep();
                                                        nextIsChar = true;
                                                    }
                                                }

                                                uint numAvailablebytesFull = _matchFinder.GetNumAvailablebytes() + 1;
                                                numAvailablebytesFull = LzmaHelpers.GetMinValue(BaseConstants.OptimumNumber - 1 - cur, numAvailablebytesFull);
                                                numAvailablebytes = numAvailablebytesFull;

                                                var continueLoop = numAvailablebytes < 2;

                                                if (!continueLoop)
                                                {
                                                    if (numAvailablebytes > _numberOfFastBytes)
                                                        numAvailablebytes = _numberOfFastBytes;
                                                    if (!nextIsChar && matchbyte != currentbyte)
                                                    {
                                                        // try Literal + rep0
                                                        uint t = LzmaHelpers.GetMinValue(numAvailablebytesFull - 1, _numberOfFastBytes);
                                                        uint lenTest2 = _matchFinder.GetMatchLen(0, reps[0], t);
                                                        if (lenTest2 >= 2)
                                                        {
                                                            CoderState state2 = state;
                                                            state2.UpdateChar();
                                                            uint posStateNext = (position + 1) & _posStateMask;
                                                            uint nextRepMatchPrice = curAnd1Price +
                                                                _isMatch[(state2.Index << BaseConstants.MaxPositionStatesBits) + posStateNext].GetPrice1() +
                                                                _isRep[state2.Index].GetPrice1();
                                                            {
                                                                uint offset = cur + 1 + lenTest2;
                                                                while (lenEnd < offset)
                                                                    _optimum[++lenEnd].Price = IfinityPrice;
                                                                uint curAndLenPrice = nextRepMatchPrice + GetRepPrice(
                                                                    0, lenTest2, state2, posStateNext);
                                                                Optimal optimum = _optimum[offset];
                                                                if (curAndLenPrice < optimum.Price)
                                                                {
                                                                    optimum.Price = curAndLenPrice;
                                                                    optimum.PosPrev = cur + 1;
                                                                    optimum.BackPrev = 0;
                                                                    optimum.Prev1IsChar = true;
                                                                    optimum.Prev2 = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                if (!continueLoop)
                                                {
                                                    uint startLen = 2; // speed optimization 

                                                    for (uint repIndex = 0; !continueLoop && repIndex < BaseConstants.RepDistances; repIndex++)
                                                    {
                                                        uint lenTest = _matchFinder.GetMatchLen(0 - 1, reps[repIndex], numAvailablebytes);

                                                        continueLoop = lenTest < 2;

                                                        if (!continueLoop)
                                                        {
                                                            uint lenTestTemp = lenTest;
                                                            do
                                                            {
                                                                while (lenEnd < cur + lenTest)
                                                                    _optimum[++lenEnd].Price = IfinityPrice;
                                                                uint curAndLenPrice = repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState);
                                                                Optimal optimum = _optimum[cur + lenTest];
                                                                if (curAndLenPrice < optimum.Price)
                                                                {
                                                                    optimum.Price = curAndLenPrice;
                                                                    optimum.PosPrev = cur;
                                                                    optimum.BackPrev = repIndex;
                                                                    optimum.Prev1IsChar = false;
                                                                }
                                                            }
                                                            while (--lenTest >= 2);
                                                            lenTest = lenTestTemp;

                                                            if (repIndex == 0)
                                                                startLen = lenTest + 1;

                                                            // if (_maxMode)
                                                            if (lenTest < numAvailablebytesFull)
                                                            {
                                                                uint t = LzmaHelpers.GetMinValue(numAvailablebytesFull - 1 - lenTest, _numberOfFastBytes);
                                                                uint lenTest2 = _matchFinder.GetMatchLen((int)lenTest, reps[repIndex], t);
                                                                if (lenTest2 >= 2)
                                                                {
                                                                    CoderState state2 = state;
                                                                    state2.UpdateRep();
                                                                    uint posStateNext = (position + lenTest) & _posStateMask;
                                                                    uint curAndLenCharPrice =
                                                                            repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState) +
                                                                            _isMatch[(state2.Index << BaseConstants.MaxPositionStatesBits) + posStateNext].GetPrice0() +
                                                                            _literalEncoder.GetSubCoder(position + lenTest,
                                                                            _matchFinder.GetIndexbyte((int)lenTest - 1 - 1)).GetPrice(true,
                                                                            _matchFinder.GetIndexbyte((int)((int)lenTest - 1 - (int)(reps[repIndex] + 1))),
                                                                            _matchFinder.GetIndexbyte((int)lenTest - 1));
                                                                    state2.UpdateChar();
                                                                    posStateNext = (position + lenTest + 1) & _posStateMask;
                                                                    uint nextMatchPrice = curAndLenCharPrice + _isMatch[(state2.Index << BaseConstants.MaxPositionStatesBits) + posStateNext].GetPrice1();
                                                                    uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                                                                    // for(; lenTest2 >= 2; lenTest2--)
                                                                    {
                                                                        uint offset = lenTest + 1 + lenTest2;
                                                                        while (lenEnd < cur + offset)
                                                                            _optimum[++lenEnd].Price = IfinityPrice;
                                                                        uint curAndLenPrice = nextRepMatchPrice + GetRepPrice(0, lenTest2, state2, posStateNext);
                                                                        Optimal optimum = _optimum[cur + offset];
                                                                        if (curAndLenPrice < optimum.Price)
                                                                        {
                                                                            optimum.Price = curAndLenPrice;
                                                                            optimum.PosPrev = cur + lenTest + 1;
                                                                            optimum.BackPrev = 0;
                                                                            optimum.Prev1IsChar = true;
                                                                            optimum.Prev2 = true;
                                                                            optimum.PosPrev2 = cur;
                                                                            optimum.BackPrev2 = repIndex;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }

                                                    if (!continueLoop)
                                                    {
                                                        if (newLen > numAvailablebytes)
                                                        {
                                                            newLen = numAvailablebytes;
                                                            for (numDistancePairs = 0; newLen > _matchDistances[numDistancePairs]; numDistancePairs += 2) ;
                                                            _matchDistances[numDistancePairs] = newLen;
                                                            numDistancePairs += 2;
                                                        }
                                                        if (newLen >= startLen)
                                                        {
                                                            normalMatchPrice = matchPrice + _isRep[state.Index].GetPrice0();
                                                            while (lenEnd < cur + newLen)
                                                                _optimum[++lenEnd].Price = IfinityPrice;

                                                            uint offs = 0;
                                                            while (startLen > _matchDistances[offs])
                                                                offs += 2;

                                                            var run2 = true;
                                                            var lenTest = startLen;
                                                            while (run2)
                                                            {
                                                                uint curBack = _matchDistances[offs + 1];
                                                                uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(curBack, lenTest, posState);
                                                                Optimal optimum = _optimum[cur + lenTest];
                                                                if (curAndLenPrice < optimum.Price)
                                                                {
                                                                    optimum.Price = curAndLenPrice;
                                                                    optimum.PosPrev = cur;
                                                                    optimum.BackPrev = curBack + BaseConstants.RepDistances;
                                                                    optimum.Prev1IsChar = false;
                                                                }

                                                                if (lenTest == _matchDistances[offs])
                                                                {
                                                                    if (lenTest < numAvailablebytesFull)
                                                                    {
                                                                        uint t = LzmaHelpers.GetMinValue(numAvailablebytesFull - 1 - lenTest, _numberOfFastBytes);
                                                                        uint lenTest2 = _matchFinder.GetMatchLen((int)lenTest, curBack, t);
                                                                        if (lenTest2 >= 2)
                                                                        {
                                                                            CoderState state2 = state;
                                                                            state2.UpdateMatch();
                                                                            uint posStateNext = (position + lenTest) & _posStateMask;
                                                                            uint curAndLenCharPrice = curAndLenPrice +
                                                                                _isMatch[(state2.Index << BaseConstants.MaxPositionStatesBits) + posStateNext].GetPrice0() +
                                                                                _literalEncoder.GetSubCoder(position + lenTest,
                                                                                _matchFinder.GetIndexbyte((int)lenTest - 1 - 1)).
                                                                                GetPrice(true,
                                                                                _matchFinder.GetIndexbyte((int)lenTest - (int)(curBack + 1) - 1),
                                                                                _matchFinder.GetIndexbyte((int)lenTest - 1));
                                                                            state2.UpdateChar();
                                                                            posStateNext = (position + lenTest + 1) & _posStateMask;
                                                                            uint nextMatchPrice = curAndLenCharPrice + _isMatch[(state2.Index << BaseConstants.MaxPositionStatesBits) + posStateNext].GetPrice1();
                                                                            uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                                                                            uint offset = lenTest + 1 + lenTest2;
                                                                            while (lenEnd < cur + offset)
                                                                                _optimum[++lenEnd].Price = IfinityPrice;
                                                                            curAndLenPrice = nextRepMatchPrice + GetRepPrice(0, lenTest2, state2, posStateNext);
                                                                            optimum = _optimum[cur + offset];
                                                                            if (curAndLenPrice < optimum.Price)
                                                                            {
                                                                                optimum.Price = curAndLenPrice;
                                                                                optimum.PosPrev = cur + lenTest + 1;
                                                                                optimum.BackPrev = 0;
                                                                                optimum.Prev1IsChar = true;
                                                                                optimum.Prev2 = true;
                                                                                optimum.PosPrev2 = cur;
                                                                                optimum.BackPrev2 = curBack + BaseConstants.RepDistances;
                                                                            }
                                                                        }
                                                                    }
                                                                    offs += 2;
                                                                    if (offs == numDistancePairs)
                                                                        // Break.
                                                                        run2 = false;
                                                                }

                                                                // It's because we are possibly in break.
                                                                if (run2) lenTest++;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return returnValues;
        }

        private bool ChangePair(uint smallDist, uint bigDist)
        {
            const int kDif = 7;
            return (smallDist < ((uint)(1) << (32 - kDif)) && bigDist >= (smallDist << kDif));
        }

        private void WriteEndMarker(uint posState)
        {
            if (_writeEndMark)
            {

                _isMatch[(_state.Index << BaseConstants.MaxPositionStatesBits) + posState].Encode(_rangeEncoder, 1);
                _isRep[_state.Index].Encode(_rangeEncoder, 0);
                _state.UpdateMatch();
                uint len = BaseConstants.MinMatchLength;
                _lenEncoder.Encode(_rangeEncoder, len - BaseConstants.MinMatchLength, posState);
                uint posSlot = (1 << BaseConstants.PositionSlotBits) - 1;
                uint lenToPosState = BaseConstants.GetLengthToPositionState(len);
                _posSlotEncoder[lenToPosState].Encode(_rangeEncoder, posSlot);
                int footerBits = 30;
                uint posReduced = (((uint)1) << footerBits) - 1;
                _rangeEncoder.EncodeDirectBits(posReduced >> BaseConstants.AlignBits, footerBits - BaseConstants.AlignBits);
                _posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & BaseConstants.AlignMask);
            }
        }

        private void Flush(uint nowPos)
        {
            ReleaseMFStream();
            WriteEndMarker(nowPos & _posStateMask);
            _rangeEncoder.FlushData();
        }

        private void ReleaseMFStream()
        {
            if (_matchFinder != null && _needReleaseMFStream)
            {
                _matchFinder.ReleaseStream();
                _needReleaseMFStream = false;
            }
        }

        private void SetOutStream(SimpleMemoryStream outStream) { _rangeEncoder.SetStream(outStream); }
        private void ReleaseOutStream() { _rangeEncoder.ReleaseStream(); }

        private void ReleaseStreams()
        {
            ReleaseMFStream();
            ReleaseOutStream();
        }

        private void SetStreams(SimpleMemoryStream inStream, SimpleMemoryStream outStream)
        {
            _inStream = inStream;
            _finished = false;
            Create();
            SetOutStream(outStream);
            Init();

            // if (!_fastMode)
            {
                FillDistancesPrices();
                FillAlignPrices();
            }

            _lenEncoder.SetTableSize(_numberOfFastBytes + 1 - BaseConstants.MinMatchLength);
            _lenEncoder.UpdateTables((uint)1 << _posStateBits);
            _repMatchLenEncoder.SetTableSize(_numberOfFastBytes + 1 - BaseConstants.MinMatchLength);
            _repMatchLenEncoder.UpdateTables((uint)1 << _posStateBits);

            nowPos64 = 0;
        }

        private void FillDistancesPrices()
        {
            for (uint i = BaseConstants.StartPositionModelIndex; i < BaseConstants.FullDistances; i++)
            {
                uint posSlot = GetPosSlot(i);
                int footerBits = (int)((posSlot >> 1) - 1);
                uint baseVal = ((2 | (posSlot & 1)) << footerBits);
                tempPrices[i] = BitTreeEncoder.ReverseGetPrice(_posEncoders,
                    baseVal - posSlot - 1, footerBits, i - baseVal);
            }

            for (uint lenToPosState = 0; lenToPosState < BaseConstants.LengthToPositionStates; lenToPosState++)
            {
                uint posSlot;
                BitTreeEncoder encoder = _posSlotEncoder[lenToPosState];

                uint st = (lenToPosState << BaseConstants.PositionSlotBits);
                for (posSlot = 0; posSlot < _distanceTableSize; posSlot++)
                    _posSlotPrices[st + posSlot] = encoder.GetPrice(posSlot);
                for (posSlot = BaseConstants.EndPositionModelIndex; posSlot < _distanceTableSize; posSlot++)
                    _posSlotPrices[st + posSlot] += ((((posSlot >> 1) - 1) - BaseConstants.AlignBits) << RangeEncoderConstants.BitPriceShiftBits);

                uint st2 = lenToPosState * BaseConstants.FullDistances;
                uint i;
                for (i = 0; i < BaseConstants.StartPositionModelIndex; i++)
                    _distancesPrices[st2 + i] = _posSlotPrices[st + i];
                for (; i < BaseConstants.FullDistances; i++)
                    _distancesPrices[st2 + i] = _posSlotPrices[st + GetPosSlot(i)] + tempPrices[i];
            }
            _matchPriceCount = 0;
        }

        private void FillAlignPrices()
        {
            for (uint i = 0; i < BaseConstants.AlignTableSize; i++) _alignPrices[i] = _posAlignEncoder.ReverseGetPrice(i);

            _alignPriceCount = 0;
        }


        private class LiteralEncoder
        {
            // Literal position bits can be 2 for 32-bit data and 0 for other cases.
            // Literal context bits can be 0 for 32-bit data and 3 for other cases.
            // Either case the sum of these bits are maximum 3 so we need a static array for 1 << 3 coders.
            private const byte MaxNumberOfCoders = 1 << 3;


            private Encoder2[] _coders;
            private int _numPrevBits;
            private int _numPosBits;
            private uint _posMask;


            public void Create(int numPosBits, int numPrevBits)
            {
                _coders = new Encoder2[MaxNumberOfCoders];

                if (_numPrevBits != numPrevBits || _numPosBits != numPosBits)
                {
                    _numPosBits = numPosBits;
                    _posMask = ((uint)1 << numPosBits) - 1;
                    _numPrevBits = numPrevBits;
                    uint numStates = (uint)1 << (_numPrevBits + _numPosBits);
                    for (uint i = 0; i < numStates; i++)
                    {
                        _coders[i] = new Encoder2();
                        _coders[i].Create();
                    }
                }
            }

            public void Init(uint[] probPrices)
            {
                uint numStates = (uint)1 << (_numPrevBits + _numPosBits);
                for (uint i = 0; i < numStates; i++)
                    _coders[i].Init(probPrices);
            }

            public Encoder2 GetSubCoder(uint pos, byte prevbyte)
            {
                return _coders[((pos & _posMask) << _numPrevBits) + (uint)(prevbyte >> (8 - _numPrevBits))];
            }


            public class Encoder2
            {
                private BitEncoder[] _encoders;


                public void Create() { _encoders = new BitEncoder[0x300]; }

                public void Init(uint[] probPrices)
                {
                    for (int i = 0; i < 0x300; i++)
                    {
                        _encoders[i] = new BitEncoder();
                        _encoders[i].Init(probPrices);
                    }
                }

                public void Encode(RangeEncoder rangeEncoder, byte symbol)
                {
                    uint context = 1;
                    for (int i = 7; i >= 0; i--)
                    {
                        uint bit = (uint)((symbol >> i) & 1);
                        _encoders[context].Encode(rangeEncoder, bit);
                        context = (context << 1) | bit;
                    }
                }

                public void EncodeMatched(RangeEncoder rangeEncoder, byte matchbyte, byte symbol)
                {
                    uint context = 1;
                    bool same = true;
                    for (int i = 7; i >= 0; i--)
                    {
                        uint bit = (uint)((symbol >> i) & 1);
                        uint state = context;
                        if (same)
                        {
                            uint matchBit = (uint)((matchbyte >> i) & 1);
                            state += ((1 + matchBit) << 8);
                            same = (matchBit == bit);
                        }
                        _encoders[state].Encode(rangeEncoder, bit);
                        context = (context << 1) | bit;
                    }
                }

                public uint GetPrice(bool matchMode, byte matchbyte, byte symbol)
                {
                    uint price = 0;
                    uint context = 1;
                    int i = 7;
                    if (matchMode)
                    {
                        var run = true;
                        while (run && i >= 0)
                        {
                            uint matchBit = (uint)(matchbyte >> i) & 1;
                            uint bit = (uint)(symbol >> i) & 1;
                            price += _encoders[((1 + matchBit) << 8) + context].GetPrice(bit);
                            context = (context << 1) | bit;
                            if (matchBit != bit)
                            {
                                // Break.
                                run = false;
                            }

                            i--;
                        }
                    }
                    for (; i >= 0; i--)
                    {
                        uint bit = (uint)(symbol >> i) & 1;
                        price += _encoders[context].GetPrice(bit);
                        context = (context << 1) | bit;
                    }
                    return price;
                }
            }
        }


        private class LenPriceTableEncoder
        {
            private uint[] _prices = new uint[BaseConstants.LenSymbols << BaseConstants.MaxPositionStatesEncodingBits];
            private uint _tableSize;
            private uint[] _counters = new uint[BaseConstants.MaxPositionStatesEncoding];

            #region LenEncoder fields

            private BitEncoder _choice;
            private BitEncoder _choice2;
            private BitTreeEncoder[] _lowCoder;
            private BitTreeEncoder[] _midCoder;
            private BitTreeEncoder _highCoder;

            #endregion


            public LenPriceTableEncoder()
            {
                _choice = new BitEncoder();
                _choice2 = new BitEncoder();
            }


            public void SetTableSize(uint tableSize) { _tableSize = tableSize; }

            public uint GetPrice(uint symbol, uint posState)
            {
                return _prices[posState * BaseConstants.LenSymbols + symbol];
            }

            public void UpdateTables(uint numPosStates)
            {
                for (uint posState = 0; posState < numPosStates; posState++)
                    UpdateTable(posState);
            }

            public void Encode(RangeEncoder rangeEncoder, uint symbol, uint posState)
            {
                EncodeLenEncoder(rangeEncoder, symbol, posState);
                if (--_counters[posState] == 0)
                    UpdateTable(posState);
            }


            private void UpdateTable(uint posState)
            {
                SetPricesLenEncoder(posState, _tableSize, _prices, posState * BaseConstants.LenSymbols);
                _counters[posState] = _tableSize;
            }


            #region LenEncoder Methods

            public void InitLenEncoder(uint numPosStates, uint[] probPrices)
            {
                _lowCoder = new BitTreeEncoder[BaseConstants.MaxPositionStatesEncoding];
                _midCoder = new BitTreeEncoder[BaseConstants.MaxPositionStatesEncoding];
                _highCoder = new BitTreeEncoder(BaseConstants.HighLengthBits);

                for (uint posState = 0; posState < BaseConstants.MaxPositionStatesEncoding; posState++)
                {
                    _lowCoder[posState] = new BitTreeEncoder(BaseConstants.LowLengthBits);
                    _midCoder[posState] = new BitTreeEncoder(BaseConstants.MidLengthBits);
                }

                _choice.Init(probPrices);
                _choice2.Init(probPrices);
                for (uint posState = 0; posState < numPosStates; posState++)
                {
                    _lowCoder[posState].Init(probPrices);
                    _midCoder[posState].Init(probPrices);
                }
                _highCoder.Init(probPrices);
            }

            private void EncodeLenEncoder(RangeEncoder rangeEncoder, uint symbol, uint posState)
            {
                if (symbol < BaseConstants.LowLengthSymbols)
                {
                    _choice.Encode(rangeEncoder, 0);
                    _lowCoder[posState].Encode(rangeEncoder, symbol);
                }
                else
                {
                    symbol -= BaseConstants.LowLengthSymbols;
                    _choice.Encode(rangeEncoder, 1);
                    if (symbol < BaseConstants.MidLengthSymbols)
                    {
                        _choice2.Encode(rangeEncoder, 0);
                        _midCoder[posState].Encode(rangeEncoder, symbol);
                    }
                    else
                    {
                        _choice2.Encode(rangeEncoder, 1);
                        _highCoder.Encode(rangeEncoder, symbol - BaseConstants.MidLengthSymbols);
                    }
                }
            }

            private void SetPricesLenEncoder(uint posState, uint numSymbols, uint[] prices, uint st)
            {
                uint a0 = _choice.GetPrice0();
                uint a1 = _choice.GetPrice1();
                uint b0 = a1 + _choice2.GetPrice0();
                uint b1 = a1 + _choice2.GetPrice1();
                uint i = 0;
                for (i = 0; i < BaseConstants.LowLengthSymbols; i++)
                {
                    if (i < numSymbols) prices[st + i] = a0 + _lowCoder[posState].GetPrice(i);
                }
                for (; i < BaseConstants.LowLengthSymbols + BaseConstants.MidLengthSymbols; i++)
                {
                    if (i < numSymbols) prices[st + i] = b0 + _midCoder[posState].GetPrice(i - BaseConstants.LowLengthSymbols);
                }
                for (; i < numSymbols; i++)
                    prices[st + i] = b1 + _highCoder.GetPrice(i - BaseConstants.LowLengthSymbols - BaseConstants.MidLengthSymbols);
            }

            #endregion
        }

        private class Optimal
        {
            public CoderState State;
            public bool Prev1IsChar;
            public bool Prev2;
            public uint PosPrev2;
            public uint BackPrev2;
            public uint Price;
            public uint PosPrev;
            public uint BackPrev;
            public uint Backs0;
            public uint Backs1;
            public uint Backs2;
            public uint Backs3;


            public void MakeAsChar() { BackPrev = 0xFFFFFFFF; Prev1IsChar = false; }

            public void MakeAsShortRep() { BackPrev = 0; ; Prev1IsChar = false; }

            public bool IsShortRep() { return (BackPrev == 0); }
        };

        public class OutResult
        {
            public uint ReturnValue { get; set; }
            public uint OutValue { get; set; }
        }
    }
}