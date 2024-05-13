/***************************************************************

•   File: PrintableEncoding.cs

•   Description.

    Represents an enum  of  encoding  types. Each   enumeration  
    element represents a specific encoding type.

 ***************************************************************/

namespace System.Text
{
    /// <summary>
    /// Defines printable encoding types.
    /// </summary>
    public enum PrintableEncoding
    {
        /// <summary>
        /// Represents a 32 character encoding.
        /// </summary>
        Base32,

        /// <summary>
        /// Represents a 64 character encoding.
        /// </summary>
        Base64,

        /// <summary>
        /// Represents the decimal number system.
        /// </summary>
        Decimal,

        /// <summary>
        /// Represents the hexadecimal encoding.
        /// </summary>
        Hexadecimal
    }
}