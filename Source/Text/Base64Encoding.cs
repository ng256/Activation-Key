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

namespace System.Text
{
  internal sealed class Base64Encoding : IBaseEncoding
  {
    public string GetString(byte[] bytes) => Convert.ToBase64String(bytes);

    public byte[] GetBytes(string s) => Convert.FromBase64String(s);

    public object Clone() => new Base64Encoding();
  }
}
