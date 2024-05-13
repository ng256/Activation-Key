/***************************************************************

•   File: IPrintableEncoding.cs

•   Description.

    IBaseEncoding provides methods for converting binary data to
    text representation.

    The GetString and GetBytes  methods allow you to  encode and
    decode data, which  can  be useful when working  with binary
    data that needs to be represented in a human-readable format
    or transmitted over pipes that only support text data.

***************************************************************/

namespace System.Text
{
  /// <summary>
  /// Represents a set of methods that allow you to convert binary data into a text representation.
  /// This can be useful for displaying binary data in a human-readable format,
  /// or for transferring binary data over pipes that only support text data.
  /// </summary>
  public interface IPrintableEncoding : ICloneable
  {
    /// <summary>Decodes all the bytes in the specified byte array into a string.</summary>
    /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
    /// <returns>A string that contains the results of decoding the specified sequence of bytes.</returns>
    string GetString(byte[] bytes);

    /// <summary>Encodes all the characters in the specified string into a sequence of bytes.</summary>
    /// <param name="s">The string containing the characters to encode.</param>
    /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
    byte[] GetBytes(string s);
  }
}
