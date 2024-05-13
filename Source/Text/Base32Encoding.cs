/***************************************************************

•   File: Base16Encoding.cs

•   Description.

    Base32Encoding  implements methods for encoding and decoding
    data using Base32 encoding.

***************************************************************/

using static System.InternalTools;

namespace System.Text
{
    internal sealed class Base32Encoding : InternalBaseEncoding
    {
        public override string EncodingName => "base-32";

        public Base32Encoding()
            : base(0)
        {
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            Validate(bytes, byteIndex, byteCount, chars, charIndex);

            int maxCharCount = GetMaxCharCount(byteCount);
            byte value = 0;
            byte bitsCount = 5;
            int startCharIndex = charIndex;
            int endByteIndex = byteIndex + byteCount;
            while (byteIndex < endByteIndex)
            {
                byte currentByte = bytes[byteIndex++];
                chars[charIndex++] = GetDigit((byte)(value | (uint)currentByte >> 8 - bitsCount));
                if (bitsCount < 4)
                {
                    chars[charIndex++] = GetDigit((byte)(currentByte >> 3 - bitsCount & 0x1F));
                    bitsCount += 5;
                }

                bitsCount -= 3;
                value = (byte)(currentByte << bitsCount & 0x1F);
            }

            if (charIndex != maxCharCount)
                chars[charIndex++] = GetDigit(value);

            return charIndex - startCharIndex;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            Validate(chars, charIndex, charCount, bytes, byteIndex);
            charCount = GetCharCount(chars, charIndex, charCount, bytes, byteIndex);
            int byteCount = bytes.GetMaxCount(byteIndex);
            int startByteIndex = byteIndex;
            int endCharIndex = charIndex + charCount;
            uint block = 0;
            byte bitsCount = 8;
            while (charIndex < endCharIndex)
            {
                int value = GetValue(chars[charIndex++]);
                if (bitsCount > 5)
                {
                    block |= (byte)(value << bitsCount - 5);
                    bitsCount -= 5;
                }
                else
                {
                    bytes[byteIndex++] = (byte)(block | (uint)(value >> 5 - bitsCount));
                    block = (byte)(value << 3 + bitsCount);
                    bitsCount += 3;
                }
            }

            if (byteIndex != byteCount)
                bytes[byteIndex] = (byte) block;

            return byteIndex - startByteIndex;
        }

        private static char GetDigit(byte value)
        {
            return value < 0x1A ? (char)(value + 0x41) : (char)(value + 0x18);
        }

        private static int GetValue(char digit)
        {
            if (digit < 0x5B && digit > 0x40) return digit - 0x41;
            if (digit < 0x7B && digit > 0x60) return digit - 0x61;
            if (digit < 0x38 && digit > 0x31) return digit - 0x18;
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
            return (int) Math.Ceiling(charCount * 5 + 4.0) / 8;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return (int) Math.Ceiling(byteCount * 8 + 4.0) / 5;
        }

        public override object Clone()
        {
            return new Base32Encoding();
        }
    }
}
