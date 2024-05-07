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

        public override string EncodingName => this._encodingName;

        public Base32Encoding()
            : base(0)
        {
            this._encodingName = "base-32";
        }

        public override int GetBytes(
            char[] chars,
            int charIndex,
            int charCount,
            byte[] bytes,
            int byteIndex)
        {
            this.Validate(chars, charIndex, charCount, bytes, byteIndex);
            charCount = this.GetCharCount(chars, charIndex, charCount, bytes, byteIndex);
            int maxCount = bytes.GetMaxCount<byte>(byteIndex);
            int num1 = charIndex;
            int num2 = num1 + charCount;
            byte num3 = 0;
            byte num4 = 8;
            while (charIndex < num2)
            {
                int num5 = Base32Encoding.GetValue(chars[charIndex++]);
                if (num4 > (byte)5)
                {
                    int num6 = num5 << (int)num4 - 5;
                    num3 |= (byte)num6;
                    num4 -= (byte)5;
                }
                else
                {
                    int num7 = num5 >> 5 - (int)num4;
                    byte num8 = (byte)((uint)num3 | (uint)num7);
                    bytes[byteIndex++] = num8;
                    num3 = (byte)(num5 << 3 + (int)num4);
                    num4 += (byte)3;
                }
            }

            if (byteIndex != maxCount)
                bytes[byteIndex] = num3;
            return charIndex - num1;
        }

        public override int GetChars(
            byte[] bytes,
            int byteIndex,
            int byteCount,
            char[] chars,
            int charIndex)
        {
            int maxCharCount = this.GetMaxCharCount(byteCount);
            byte b1 = 0;
            byte num1 = 5;
            for (int index = byteIndex; index < byteCount; ++index)
            {
                byte num2 = bytes[index];
                byte b2 = (byte)((uint)b1 | (uint)num2 >> 8 - (int)num1);
                chars[charIndex++] = Base32Encoding.GetDigit(b2);
                if (num1 < (byte)4)
                {
                    byte b3 = (byte)((int)num2 >> 3 - (int)num1 & 31);
                    chars[charIndex++] = Base32Encoding.GetDigit(b3);
                    num1 += (byte)5;
                }

                num1 -= (byte)3;
                b1 = (byte)((int)num2 << (int)num1 & 31);
            }

            if (charIndex != maxCharCount)
                chars[charIndex++] = Base32Encoding.GetDigit(b1);
            return charIndex;
        }

        public override int GetByteCount(char[] chars, int index, int count) =>
            this.GetMaxByteCount(chars.Length - index);

        public override int GetCharCount(byte[] bytes, int index, int count) =>
            this.GetMaxCharCount(bytes.Length - index);

        public override int GetMaxByteCount(int charCount) => (int)Math.Ceiling((double)charCount * 5.0 + 4.0) / 8;

        public override int GetMaxCharCount(int byteCount) => (int)Math.Ceiling((double)byteCount * 8.0 + 4.0) / 5;

        private static int GetValue(char digit)
        {
            if (digit < 0x5B && digit > 0x40) return digit - 0x41;
            if (digit < 0x38 && digit > 0x31) return digit - 0x18;
            if (digit < 0x7B && digit > 0x60) return digit - 0x61;
            throw new ArgumentOutOfRangeException(nameof(digit), digit, GetResourceString("Format_BadBase"));
        }

        private static char GetDigit(byte b)
        {
            return b < 0x1A ? (char)(b + 0x41) : (char)(b + 0x18);
        }

        public override object Clone()
        {
            return new Base32Encoding();
        }
    }
}
