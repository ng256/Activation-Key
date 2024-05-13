/***************************************************************

•   File: IniFile.cs

•   Description.

    BaseEncoding provides  a convenient  way  to create  and use
    various    encodings   that   implement   the  IBaseEncoding
    interface, which can be useful   in a variety  of scenarios.

    The   BaseEncoding class implements the following encodings:

       - Base16Encoding - Returns the hexadecimal encoding.
       - Base10Encoding - Returns the decimal encoding.
       - Base32Encoding - Returns the 32-digit encoding.
       - Base64Encoding - Returns the 64-character encoding.

    Also  in  the  BaseEncoding  class   there is  a GetEncoding
    method,  which takes an alphabet string  as a  parameter and
    creates  an  instance  of    a   class that   implements the
    IBaseEncoding  interface. This class  is based  on  a passed
    alphabet string.

***************************************************************/

namespace System.Text
{
  /// <summary>
  /// The BaseEncoding class provides static methods for creating and obtaining instances of classes that implement the IBaseEncoding interface.
  /// </summary>
  public static class BaseEncoding
  {
    /// <summary>
    /// Returns a hexadecimal based encoding that implements the <see cref="IBaseEncoding" /> interface.
    /// </summary>
    public static IBaseEncoding HexadecimalEncoding => (IBaseEncoding) new Base16Encoding();

    /// <summary>
    /// Returns a decimal encoding that implements the <see cref="IBaseEncoding" /> interface.
    /// </summary>
    public static IBaseEncoding DecimalEncoding => GetEncoding("0123456789");

    /// <summary>
    /// Returns a 32-digit based encoding that implements the <see cref="IBaseEncoding" /> interface.
    /// </summary>
    public static IBaseEncoding Base32Encoding => (IBaseEncoding) new Base32Encoding();

    /// <summary>
    /// Returns a 64-digit based encoding that implements the <see cref="IBaseEncoding" /> interface.
    /// </summary>
    public static IBaseEncoding Base64Encoding => (IBaseEncoding) new Base64Encoding();

    /// <summary>
    /// Creates an instance of the custom encoding class, which implements the <see cref="IBaseEncoding" /> interface,
    /// based on the passed alphabet string.
    /// </summary>
    /// <param name="alphabet">The alphabet string on the basis of which encoding will be created.</param>
    /// <returns>An instance of a class that implements the <see cref="IBaseEncoding" /> interface.</returns>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="alphabet" /> parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The <paramref name="alphabet" /> parameter was either an empty string, contained duplicates, or contained only spaces.
    /// </exception>
    public static IBaseEncoding GetEncoding(string alphabet) => (IBaseEncoding) new CustomEncoding(alphabet);

    /// <summary>
    /// Creates an instance of encoding based on the passed <see cref="BaseEncodingType" />.
    /// </summary>
    /// <param name="type">The <see cref="BaseEncodingType" /> on which the encoding will be created.</param>
    /// <returns>An instance of a class that implements the <see cref="IBaseEncoding" /> interface.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="type" /> parameter does not match any of the listed values.
    /// </exception>
    public static IBaseEncoding GetEncoding(BaseEncodingType type)
    {
      switch (type)
      {
        case BaseEncodingType.Base10:
          return Base10Encoding;
        case BaseEncodingType.Base16:
          return Base16Encoding;
        case BaseEncodingType.Base32:
          return Base32Encoding;
        case BaseEncodingType.Base64:
          return Base64Encoding;
        default:
          throw new ArgumentOutOfRangeException(nameof (type), (object) type, InternalTools.GetResourceString("Arg_EnumIllegalVal", (object) type));
      }
    }
  }
}
