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
        private readonly string _encodingName;

        public override string EncodingName => _encodingName;

        public Base32Encoding()
            : base(0)
        {
            _encodingName = "base-32";
        }

        public override int GetBytes(
            char[] chars,
            int charIndex,
            int charCount,
            byte[] bytes,
            int byteIndex)
        {
            Validate(chars, charIndex, charCount, bytes, byteIndex);
            charCount = GetCharCount(chars, charIndex, charCount, bytes, byteIndex);
            int maxCount = bytes.GetMaxCount(byteIndex);
            int startByteIndex = byteIndex;
            int endCharIndex = charIndex + charCount;
            byte currentByte = 0;
            byte bitsCount = 8;
            while (charIndex < endCharIndex)
            {
                int value = GetValue(chars[charIndex++]);
                if (bitsCount > 5)
                {
                    int num6 = value << bitsCount - 5;
                    currentByte |= (byte)num6;
                    bitsCount -= 5;
                }
                else
                {
                    int num7 = value >> 5 - bitsCount;
                    byte num8 = (byte)(currentByte | (uint)num7);
                    bytes[byteIndex++] = num8;
                    currentByte = (byte)(value << 3 + bitsCount);
                    bitsCount += 3;
                }
            }

            if (byteIndex != maxCount)
                bytes[byteIndex] = currentByte;
            return byteIndex - startByteIndex;
        }

        public override int GetChars(
            byte[] bytes,
            int byteIndex,
            int byteCount,
            char[] chars,
            int charIndex)
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
                byte b2 = (byte)(value | (uint)currentByte >> 8 - bitsCount);
                chars[charIndex++] = GetDigit(b2);
                if (bitsCount < 4)
                {
                    byte b3 = (byte)(currentByte >> 3 - bitsCount & 31);
                    chars[charIndex++] = GetDigit(b3);
                    bitsCount += 5;
                }

                bitsCount -= 3;
                value = (byte)(currentByte << bitsCount & 31);
            }

            if (charIndex != maxCharCount)
                chars[charIndex++] = GetDigit(value);
            return charIndex - startCharIndex;
        }

        public override int GetByteCount(char[] chars, int index, int count) =>
            GetMaxByteCount(chars.Length - index);

        public override int GetCharCount(byte[] bytes, int index, int count) =>
            GetMaxCharCount(bytes.Length - index);

        public override int GetMaxByteCount(int charCount) => (int)Math.Ceiling(charCount * 5.0 + 4.0) / 8;

        public override int GetMaxCharCount(int byteCount) => (int)Math.Ceiling(byteCount * 8.0 + 4.0) / 5;

        private static int GetValue(char digit)
        {
            if (digit < 0x5B && digit > 0x40) return digit - 0x41;
            if (digit < 0x38 && digit > 0x31) return digit - 0x18;
            if (digit < 0x7B && digit > 0x60) return digit - 0x61;
            throw new ArgumentOutOfRangeException(nameof(digit), digit, GetResourceString("Format_BadBase"));
        }

        private static char GetDigit(byte value)
        {
            return value < 0x1A ? (char)(value + 0x41) : (char)(value + 0x18);
        }

        public override object Clone()
        {
            return new Base32Encoding();
        }
    }
}
