/***************************************************************

•   File: Base16Encoding.cs

•   Description.

    Base64Encoding  implements methods for encoding and decoding
    data using Base64 encoding.

***************************************************************/

using static System.InternalTools;

namespace System.Text
{
    internal sealed class Base64Encoding : InternalBaseEncoding
    {
        protected override int EncodingBlockSize => 3;
        protected override int DecodingBlockSize => 4;
        protected override int EncodingBits => 6;
        public override string EncodingName => "base-64";

        public Base64Encoding() : base(0)
        {
        }

        public override string GetString(byte[] bytes)
        {
            return ToBase64String(bytes);
        }

        public override byte[] GetBytes(string s)
        {
            return FromBase64String(s);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            Validate(bytes, byteIndex, byteCount, chars, charIndex);
            return ToBase64CharArray(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return GetMaxByteCount(chars.Length - index);
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return GetMaxCharCount(bytes.Length - index);
        }


        public override int GetMaxByteCount(int charCount)
        {
            int maxByteCount = charCount / 4 * 3;
            return maxByteCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount / 3 * 4;
        }

        public char GetDigit(int value)
        {
            if (value < 0x1A) return (char) (value + 0x41);
            if (value < 0x34) return (char) (value + 0x47);
            if (value < 0x3E) return (char) (value - 0x4);
            if (value == 0x3E) return (char)0x2B;
            if (value == 0x3F) return (char)0x2F;
            throw new ArgumentOutOfRangeException(nameof(value), value, GetResourceString("Format_BadBase"));
        }

        private int GetValue(char digit)
        {
            if (digit < 0x5B && digit > 0x40) return digit - 0x41;
            if (digit < 0x7B && digit > 0x60) return digit - 0x47;
            if (digit < 0x3A && digit > 0x31) return digit + 4;
            if (digit == 0x2B) return 0x3E;
            if (digit == 0x2F) return 0x3F;
            throw new ArgumentOutOfRangeException(nameof(digit), digit, GetResourceString("Format_BadBase"));
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            Validate(chars, charIndex, charCount, bytes, byteIndex);
            byte[] buffer = FromBase64CharArray(chars, charIndex, charCount);
            buffer.CopyTo(bytes, byteIndex);
            return buffer.Length;
        }

        public override object Clone()
        {
            return new Base64Encoding();
        }

        private string ToBase64String(byte[] inArray)
        {
            if (inArray == null)
                throw new ArgumentNullException(nameof(inArray));
            return ToBase64String(inArray, 0, inArray.Length);
        }

        public  unsafe string ToBase64String(
          byte[] inArray,
          int offset,
          int length)
        {
            char[] base64String = new char[GetMaxCharCount(length)];
            fixed (char* outChars = base64String)
            fixed (byte* inData = inArray)
            {
                ConvertToBase64Array(outChars, inData, offset, length);
                return new string(base64String);
            }
        }

        private unsafe int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut)
        {
            fixed (char* outChars = &outArray[offsetOut])
            fixed (byte* inData = inArray)
                return ConvertToBase64Array(outChars, inData, offsetIn, length);
        }

        private unsafe int ConvertToBase64Array(char* chars, byte* bytes, int offset, int length)
        {
            int mod = length % 0x3;
            int endIndex = offset + (length - mod);
            int charIndex = 0x0;
            
            {
                for (int index = offset; index < endIndex; index += 0x3)
                {
                    chars[charIndex] = GetDigit((bytes[index] & 0xFC) >> 2);
                    chars[charIndex + 1] = GetDigit((bytes[index] & 0x3) << 4 | (bytes[index + 1] & 0xF0) >> 4);
                    chars[charIndex + 2] = GetDigit((bytes[index + 0x1] & 0xF) << 2 | (bytes[index + 2] & 0xC0) >> 6);
                    chars[charIndex + 3] = GetDigit(bytes[index + 0x2] & 0x3F);
                    charIndex += 4;
                }
                switch (mod)
                {
                    case 1:
                        chars[charIndex] = GetDigit((bytes[endIndex] & 0xFC) >> 2);
                        chars[charIndex + 1] = GetDigit((bytes[endIndex] & 0x3) << 4);
                        charIndex += 2;
                        break;
                    case 2:
                        chars[charIndex] = GetDigit((bytes[endIndex] & 0xFC) >> 2);
                        chars[charIndex + 1] = GetDigit((bytes[endIndex] & 0x3) << 4 | (bytes[endIndex + 0x1] & 0xF0) >> 4);
                        chars[charIndex + 2] = GetDigit((bytes[endIndex + 0x1] & 0xF) << 2);
                        charIndex += 3;
                        break;
                }
            }
            return charIndex;
        }

        private unsafe byte[] FromBase64String(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            fixed (char* inputPtr = s)
                return FromBase64CharPtr(inputPtr, s.Length);
        }

        private unsafe byte[] FromBase64CharArray(char[] inArray, int offset, int length)
        {
            fixed (char* chPtr = inArray)
                return FromBase64CharPtr(chPtr + offset, length);
        }

        private  unsafe byte[] FromBase64CharPtr(char* inputPtr, int inputLength)
        { 
            int resultLength = GetMaxByteCount(inputLength);
            byte[] numArray = new byte[resultLength];
            fixed (byte* startDestPtr = numArray)
                FromBase64_Decode(inputPtr, inputLength, startDestPtr, resultLength);
            return numArray;
        }

        private  unsafe int FromBase64_Decode(char* startInputPtr, int inputLength, byte* startDestPtr, int destLength)
        {
            char* startCharPtr = startInputPtr;
            byte* startBytePtr = startDestPtr;
            char* endCharPtr = startCharPtr + inputLength;
            byte* endBytePtr = startBytePtr + destLength;
            uint block = byte.MaxValue;
            while (startCharPtr < endCharPtr)
            {
                uint digit = *startCharPtr;
                ++startCharPtr;
                uint value;

                value = (uint) GetValue((char)digit);
                block = block << 6 | value;
                if (((int)block & int.MinValue) != 0)
                {
                    if ((int)(endBytePtr - startBytePtr) < 3)
                        return -1;
                    *startBytePtr = (byte)(block >> 16);
                    startBytePtr[1] = (byte)(block >> 8);
                    startBytePtr[2] = (byte)block;
                    startBytePtr += 3;
                    block = byte.MaxValue;
                }
            }
            if (block != byte.MaxValue)
                throw new FormatException(GetResourceString("Format_BadBase64CharArrayLength"));
            return (int)(startBytePtr - startDestPtr);
        }
    }
}

