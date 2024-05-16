/***************************************************************

•   File: Base64Encoding.cs

•   Description.

    Base64Encoding  implements methods for encoding and decoding
    data using Base64 encoding. To make the text easier to read, 
    the '+' and '/' symbols were replaced with '#' and '$', and 
    the trailing symbol '=' were removed.

***************************************************************/

using static System.InternalTools;

namespace System.Text
{
    internal sealed class Base64Encoding : InternalBaseEncoding
    {
        public override string EncodingName => "base-64";

        public Base64Encoding() : base(0)
        {
        }

        public override byte[] GetBytes(string s)
        {
            return base.GetBytes(s.Trim('='));
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            Validate(bytes, byteIndex, byteCount, chars, charIndex);

            int mod = byteCount % 3;
            int startCharIndex = charIndex;
            int endByteIndex = byteIndex + (byteCount - mod);

            while (byteIndex < endByteIndex)
            {
                chars[charIndex] = GetDigit((bytes[byteIndex] & 0xFC) >> 2);
                chars[charIndex + 1] = GetDigit((bytes[byteIndex] & 0x03) << 4 | (bytes[byteIndex + 1] & 0xF0) >> 4);
                chars[charIndex + 2] = GetDigit((bytes[byteIndex + 0x1] & 0xF) << 2 | (bytes[byteIndex + 2] & 0xC0) >> 6);
                chars[charIndex + 3] = GetDigit(bytes[byteIndex + 0x2] & 0x3F);
                byteIndex += 3;
                charIndex += 4;
            }

            switch (mod)
            {
                case 1:
                    chars[charIndex] = GetDigit((bytes[endByteIndex] & 0xFC) >> 2);
                    chars[charIndex + 1] = GetDigit((bytes[endByteIndex] & 0x3) << 4);
                    charIndex += 2;
                    break;
                case 2:
                    chars[charIndex] = GetDigit((bytes[endByteIndex] & 0xFC) >> 2);
                    chars[charIndex + 1] = GetDigit((bytes[endByteIndex] & 0x3) << 4 | (bytes[endByteIndex + 0x1] & 0xF0) >> 4);
                    chars[charIndex + 2] = GetDigit((bytes[endByteIndex + 1] & 0xF) << 2);
                    charIndex += 3;
                    break;
            }

            return charIndex - startCharIndex;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            Validate(chars, charIndex, charCount, bytes, byteIndex);

            int mod = GetMaxByteCount(charCount) % 3;
            int startByteIndex = byteIndex;
            int endCharIndex = charIndex + charCount;
            uint block = byte.MaxValue;

            while (charIndex < endCharIndex)
            {
                uint value = (uint) GetValue(chars[charIndex++]);
                block = block << 6 | value;

                if ((block & 0x80000000U) != 0)
                {
                    bytes[byteIndex] = (byte)(block >> 16);
                    bytes[byteIndex + 1] = (byte)(block >> 8);
                    bytes[byteIndex + 2] = (byte)block;

                    byteIndex += 3;
                    block = 0xFF;
                }
            }

            switch (mod)
            {
                case 1:
                    bytes[byteIndex] = (byte)(block >> 4);
                    byteIndex += 1;
                    break;
                case 2:
                    bytes[byteIndex] = (byte)(block >> 10);
                    bytes[byteIndex + 1] = (byte)(block >> 2);
                    byteIndex += 2;
                    break;
            }

            return byteIndex - startByteIndex;
        }

        public char GetDigit(int value)
        {
            if (value < 0x1A) return (char)(value + 0x41);
            if (value < 0x34) return (char)(value + 0x47);
            if (value < 0x3E) return (char)(value - 0x04);
            if (value == 0x3E) return (char)0x23;
            if (value == 0x3F) return (char)0x24;
            return (char)0x3D;
        }

        private int GetValue(char digit)
        {
            if (digit < 0x5B && digit > 0x40) return digit - 0x41;
            if (digit < 0x7B && digit > 0x60) return digit - 0x47;
            if (digit < 0x3A && digit > 0x2F) return digit + 0x04;
            if (digit == 0x23) return 0x3E;
            if (digit == 0x24) return 0x3F;
            throw new ArgumentOutOfRangeException(nameof(digit), digit, GetResourceString("Format_BadBase"));
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
            return (charCount * 3) >> 2;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return ((byteCount << 2) | 2) / 3;
        }

        public override object Clone()
        {
            return new Base64Encoding();
        }
    }
}