/********************************************************************

    •   File: CustomEncoding.cs

    •   Description.

        The  CustomEncoding class  provides    an implementation  of
        various   encodings of  binary   data to text   using custom
        characters.

        It is used to convert binary data to text representation and
        vice  versa. The class   uses a  custom alphabet,   which is
        specified when the instance is created.

        This code is based on https://github.com/KvanTTT/BaseNcoding
        distributed under Apache-2.0 license
        http://www.apache.org/licenses/LICENSE-2.0
        
   
 ******************************************************************/

using System.Linq;
using static System.InternalTools;

namespace System.Text
{

    // Represents the implementation of various encodings of binary data to text using user-specified characters.
    internal sealed class CustomEncoding : IPrintableEncoding
    {
        private const uint MAX_BITS_COUNT = 32;
        private readonly int _blockSize;
        private readonly int _blockCharsCount;
        private readonly char[] _encodingTable;
        private readonly int[] _decodingTable;
        private readonly ulong[] _powN;

        public CustomEncoding(string alphabet)
        {
            if (alphabet == null)
                throw new ArgumentNullException(nameof(alphabet), GetResourceString("ArgumentNull_String"));
            if (alphabet.IsNullOrWhiteSpace())
                throw new ArgumentException(nameof(alphabet), GetResourceString("Format_EmptyInputString"));
            _encodingTable = alphabet.ToCharArray();
            if (!alphabet.IsPrintableCharacters() || _encodingTable.ContainsDuplicates())
                throw new ArgumentException(GetResourceString("Format_InvalidString"), nameof(alphabet));

            uint charsCount = (uint)alphabet.Length;
            uint x = charsCount;
            int bitsPerChar = 0;
            while ((x >>= 1) != 0)
                bitsPerChar++;
            int lcm = bitsPerChar;
            for (int i = 8, j = i; i != 0; lcm = j)
            {
                i = lcm % i;
            }

            _blockSize = (bitsPerChar / lcm) * 8;
            _blockCharsCount = _blockSize / bitsPerChar;

            _decodingTable = new int[_encodingTable.Max() + 1];

            for (int i = 0; i < _decodingTable.Length; i++)
            {
                _decodingTable[i] = -1;
            }

            for (int i = 0; i < charsCount; i++)
            {
                _decodingTable[_encodingTable[i]] = i;
            }

            int optimalBitsCount = 0;
            uint charsCountInBits = 0;

            int logBaseN = 0;
            for (uint i = charsCount; (i /= 2) != 0; logBaseN++) ;

            double charsCountLog = Math.Log(2, charsCount);
            double maxRatio = 0;

            for (int i = logBaseN; i <= MAX_BITS_COUNT; i++)
            {
                uint j = (uint)Math.Ceiling(i * charsCountLog);
                double ratio = (double)i / j;
                if (ratio > maxRatio)
                {
                    maxRatio = ratio;
                    optimalBitsCount = i;
                    charsCountInBits = j;
                }
            }

            _blockSize = optimalBitsCount;
            _blockCharsCount = (int)charsCountInBits;
            _powN = new ulong[_blockCharsCount];
            ulong pow = 1;
            for (int i = 0; i < _blockCharsCount - 1; i++)
            {
                _powN[_blockCharsCount - 1 - i] = pow;
                pow *= charsCount;
            }

            _powN[0] = pow;
        }

        public string GetString(byte[] data)
        {

            if (data == null || data.Length == 0)
            {
                return "";
            }

            int mainBitsLength = data.Length * 8 / _blockSize * _blockSize;
            int tailBitsLength = data.Length * 8 - mainBitsLength;
            int mainCharsCount = mainBitsLength * _blockCharsCount / _blockSize;
            int tailCharsCount = (tailBitsLength * _blockCharsCount + _blockSize - 1) / _blockSize;
            int totalCharsCount = mainCharsCount + tailCharsCount;
            int iterationCount = mainCharsCount / _blockCharsCount;

            var result = new char[totalCharsCount];

            for (int ind = 0; ind < iterationCount; ind++)
            {
                int charInd = ind * _blockCharsCount;
                int bitInd = ind * _blockSize;
                ulong bits = ReadValue(data, bitInd, _blockSize);
                EncodeBlock(result, charInd, _blockCharsCount, bits);
            }

            if (tailBitsLength != 0)
            {
                ulong bits = ReadValue(data, mainBitsLength, tailBitsLength);
                EncodeBlock(result, mainCharsCount, tailCharsCount, bits);
            }

            return new string(result);
        }

        public byte[] GetBytes(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return new byte[0];
            }

            int totalBitsLength = ((str.Length - 1) * _blockSize / _blockCharsCount + 8) / 8 * 8;
            int mainBitsLength = totalBitsLength / _blockSize * _blockSize;
            int tailBitsLength = totalBitsLength - mainBitsLength;
            int mainCharsCount = mainBitsLength * _blockCharsCount / _blockSize;
            int tailCharsCount = (tailBitsLength * _blockCharsCount + _blockSize - 1) / _blockSize;
            ulong tailBits = DecodeBlock(str, mainCharsCount, tailCharsCount);
            if (tailBits >> tailBitsLength != 0)
            {
                totalBitsLength += 8;
                mainBitsLength = totalBitsLength / _blockSize * _blockSize;
                tailBitsLength = totalBitsLength - mainBitsLength;
                mainCharsCount = mainBitsLength * _blockCharsCount / _blockSize;
                tailCharsCount = (tailBitsLength * _blockCharsCount + _blockSize - 1) / _blockSize;
            }

            int iterationCount = mainCharsCount / _blockCharsCount;
            byte[] result = new byte[totalBitsLength / 8];

            for (int ind = 0; ind < iterationCount; ind++)
            {
                int charInd = ind * _blockCharsCount;
                int bitInd = ind * _blockSize;
                ulong block = DecodeBlock(str, charInd, _blockCharsCount);
                WriteValue(result, block, bitInd, _blockSize);
            }

            if (tailCharsCount != 0)
            {
                ulong block = DecodeBlock(str, mainCharsCount, tailCharsCount);
                WriteValue(result, block, mainBitsLength, tailBitsLength);
            }

            return result;
        }

        private static ulong ReadValue(byte[] data, int bitIndex, int bitsCount)
        {
            ulong result = 0;

            int currentBytePos = Math.DivRem(bitIndex, 8, out int currentBitInBytePos);

            int xLength = Math.Min(bitsCount, 8 - currentBitInBytePos);
            if (xLength != 0)
            {
                result = ((ulong)data[currentBytePos] << 0b111000 + currentBitInBytePos) >> 0x40 - xLength <<
                         bitsCount - xLength;

                currentBytePos += Math.DivRem(currentBitInBytePos + xLength, 8, out currentBitInBytePos);

                int x2Length = bitsCount - xLength;
                if (x2Length > 8)
                {
                    x2Length = 8;
                }

                while (x2Length > 0)
                {
                    xLength += x2Length;
                    result |= (ulong)data[currentBytePos] >> 8 - x2Length << bitsCount - xLength;

                    currentBytePos += Math.DivRem(currentBitInBytePos + x2Length, 8, out currentBitInBytePos);

                    x2Length = bitsCount - xLength;
                    if (x2Length > 8)
                    {
                        x2Length = 8;
                    }
                }
            }

            return result;
        }

        private static void WriteValue(byte[] data, ulong value, int bitIndex, int bitsCount)
        {
            unchecked
            {
                int currentBytePos = Math.DivRem(bitIndex, 8, out int currentBitInBytePos);

                int xLength = Math.Min(bitsCount, 8 - currentBitInBytePos);
                if (xLength != 0)
                {
                    byte x1 = (byte)(value << 64 - bitsCount >> 56 + currentBitInBytePos);
                    data[currentBytePos] |= x1;

                    currentBytePos += Math.DivRem(currentBitInBytePos + xLength, 8, out currentBitInBytePos);

                    int x2Length = bitsCount - xLength;
                    if (x2Length > 8)
                    {
                        x2Length = 8;
                    }

                    while (x2Length > 0)
                    {
                        xLength += x2Length;
                        byte x2 = (byte)(value >> bitsCount - xLength << 8 - x2Length);
                        data[currentBytePos] |= x2;

                        currentBytePos += Math.DivRem(currentBitInBytePos + x2Length, 8, out currentBitInBytePos);

                        x2Length = bitsCount - xLength;
                        if (x2Length > 8)
                        {
                            x2Length = 8;
                        }
                    }
                }
            }
        }

        private void EncodeBlock(char[] chars, int charIndex, int charCount, ulong block)
        {
            uint baseEncoding = (uint)_encodingTable.Length;
            int startCharIndex = charIndex;
            int endCharIndex = startCharIndex + charCount;
            while(charIndex < endCharIndex)
            {
                ulong blockCount = block / baseEncoding;
                ulong digit = block - blockCount * baseEncoding;
                block = blockCount;
                chars[charIndex++] = _encodingTable[(int)digit];
            }
        }

        private ulong DecodeBlock(string data, int charIndex, int charCount)
        {
            ulong result = 0;
            for (int i = 0; i < charCount; i++)
            {
                result += (ulong)_decodingTable[data[charIndex + i]] *
                          _powN[_blockCharsCount - 1 - i];
            }

            return result;
        }

        public object Clone()
        {
            return new CustomEncoding(new string(_encodingTable));
        }
    }
}
