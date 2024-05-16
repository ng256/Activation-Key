/***************************************************************

•   File: InternalBaseEncoding.cs

•   Description.

    It is  an abstract  class  InternalBaseEncoding, which  is a
    descendant   of the    Encoding  class  and   implements the
    IBaseEncoding  interface for creating various encodings.

***************************************************************/

namespace System.Text
{
  internal abstract class InternalBaseEncoding : Encoding, IPrintableEncoding
  {
    protected InternalBaseEncoding(int codePage)
      : base(codePage)
    {
    }

    protected virtual void Validate(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
      if (chars == null)
        throw new ArgumentNullException(nameof (chars));
      if (charIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (charIndex), charIndex, InternalTools.GetResourceString("ArgumentOutOfRange_StartIndex"));
      if (charCount < 0)
        throw new ArgumentOutOfRangeException(nameof (charCount), charCount, InternalTools.GetResourceString("ArgumentOutOfRange_NegativeCount"));
      if (bytes == null)
        throw new ArgumentNullException(nameof (bytes));
      if (byteIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (byteIndex), byteIndex, InternalTools.GetResourceString("ArgumentOutOfRange_StartIndex"));
    }

    protected virtual void Validate(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
      if (bytes == null)
        throw new ArgumentNullException(nameof (bytes));
      if (byteCount < 0)
        throw new ArgumentOutOfRangeException(nameof (byteCount), byteCount, InternalTools.GetResourceString("ArgumentOutOfRange_NegativeCount"));
      if (byteIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (byteIndex), byteIndex, InternalTools.GetResourceString("ArgumentOutOfRange_StartIndex"));
      if (chars == null)
        throw new ArgumentNullException(nameof (chars));
      if (charIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (charIndex), charIndex, InternalTools.GetResourceString("ArgumentOutOfRange_StartIndex"));
    }

    public override int GetByteCount(char[] chars, int index, int count)
    {
        return GetMaxByteCount(chars.GetMaxCount<char>(index, count));
    }

    public override int GetCharCount(byte[] bytes, int index, int count)
    {
        return GetMaxCharCount(bytes.GetMaxCount<byte>(index, count));
    }

    protected virtual int GetCharCount(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
      int maxCharCount = GetMaxCharCount(bytes.GetMaxCount<byte>(byteIndex));
      return Math.Min(chars.GetMaxCount<char>(charIndex, charCount), maxCharCount);
    }

    protected virtual int GetByteCount(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
      int maxByteCount = GetMaxByteCount(chars.GetMaxCount<char>(charIndex));
      byteCount = Math.Min(bytes.GetMaxCount<byte>(byteIndex, byteCount), maxByteCount);
      return byteCount;
    }
  }
}
