/***************************************************************

•   File: Base16Encoding.cs

•   Description.

    Base64Encoding  implements methods for encoding and decoding
    data using Base64 encoding.

    This class  is a  wrapper for the Convert.ToBase64String and
    Convert.FromBase64String  methods, and   is   essentially  a
    temporary solution. However, as   we know, there  is nothing
    more eternal than temporary.

***************************************************************/

using static System.InternalTools;

namespace System.Text
{
    internal sealed class Base64Encoding : InternalBaseEncoding
    {
        private readonly string _encodingName;

        public override string EncodingName => _encodingName;

        public Base64Encoding() : base(0)
        {
            _encodingName = "base-64";
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return System.Convert.ToBase64CharArray(bytes, byteIndex, byteCount, chars, charIndex);
        }



        public override int GetMaxByteCount(int charCount)
        {
            return (4 * charCount / 3 + 3) & ~3;
        }


        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount / 3 * 4 + (byteCount % 3 != 0 ? 4 : 0);
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            byte[] buffer = System.Convert.FromBase64CharArray(chars, charIndex, charCount);
            buffer.CopyTo(bytes, byteIndex);
            return buffer.Length;
        }

        public override object Clone()
        {
            return new Base64Encoding();
        }

        public Base64Encoding(int codePage) : base(codePage)
        {
        }
    }
}
