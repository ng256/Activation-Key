/***************************************************************

•   File: BaseEncodingTypes.cs

•   Description.

    Represents      an    enum        of     encoding     types.
    Each   enumeration  element   represents a specific encoding
    type.

    This can be useful when working with various data types that
    require a specific encoding.

 ***************************************************************/

namespace System.Text
{
    /// <summary>
    /// Defines encoding types.
    /// </summary>
    public enum BaseEncodingType
    {

        /// <summary>
        /// Represents the decimal number system.
        /// </summary>
        Base10,

        /// <summary>
        /// Represents the hexadecimal encoding.
        /// </summary>
        Base16,

        /// <summary>
        /// Represents a 32 character encoding.
        /// </summary>
        Base32,

        /// <summary>
        /// Represents a 64 character encoding.
        /// </summary>
        Base64
    }
}