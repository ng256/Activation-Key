/***************************************************************

•   File: ActivationKeyTextParser.cs

•   Description.

    ActivationKeyTextParser is  a class  that provides tools for
    working  with text data   that  represents  activation keys.
    Activation  keys  can  be    represented  as a   sequence of
    characters     separated     by      certain     delimiters.

    The  class provides    methods   for  parsing    and parsing
    activation   keys  and  for    creating  instances    of the
    ActivationKey  class, which   is a data structure containing
    information about the activation key.

***************************************************************/

using System.IO;
using System.Text;
using static System.InternalTools;

namespace System.Security.Activation
{
    /// <summary>
    /// Represents a parser that provides tools for text data with activation keys
    /// which can be represented as a sequence of characters separated by certain delimiters.
    /// </summary>
    public class ActivationKeyTextParser
    {
        /// <summary>
        /// A constant representing the character that is used by default as a delimiter between parts of the activation key.
        /// </summary>
        internal const char DefaultDelimiter = '-';

        private readonly IBaseEncoding _encoding = BaseEncoding.Base32Encoding; // used to decode data.
        private readonly char[] _delimiters = { DefaultDelimiter }; // used to split the activation key into parts.

        /// <summary>
        /// Returns the parser with default settings.
        /// </summary>
        public static ActivationKeyTextParser DefaultParser => new ActivationKeyTextParser();

        /// <summary>
        /// Initializes a new instance of <see cref="ActivationKeyTextParser"/> using the default parameters.
        /// </summary>
        public ActivationKeyTextParser()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ActivationKeyTextParser"/> using the specified parameters.
        /// </summary>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public ActivationKeyTextParser(params char[] delimiters)
        {
            if (delimiters != null && delimiters.Length > 0)
                _delimiters = delimiters;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ActivationKeyTextParser"/>.
        /// </summary>
        /// <param name="encoding">Encoding used to encode/decode data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public ActivationKeyTextParser(IBaseEncoding encoding, params char[] delimiters) : this(delimiters)
        {
            _encoding = encoding ?? BaseEncoding.Base32Encoding;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ActivationKeyTextParser"/>.
        /// </summary>
        /// <param name="encoding">Encoding used to encode/decode data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public ActivationKeyTextParser(BaseEncodingType encoding, params char[] delimiters) : this(delimiters)
        {
            _encoding = BaseEncoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ActivationKeyTextParser"/>.
        /// </summary>
        /// <param name="alphabet">String containing characters that used to encode/decode the activation key data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public ActivationKeyTextParser(string alphabet, params char[] delimiters) : this(delimiters)
        {
            _encoding = BaseEncoding.GetEncoding(alphabet);
        }

        // Encodes the given byte array using the base encoding.
        private string GetStringSafe(byte[] bytes)
        {
            return bytes.IsNullOrEmpty() ? string.Empty : _encoding.GetString(bytes);
        }

        // Parses parts of the key as strings and fills in the activation key data.
        internal void InternalParse(ActivationKey activationKey, string data, string hash, string tail)
        {
            if (!data.IsNullOrWhiteSpace())
                activationKey.Data = _encoding.GetBytes(data.ToUpperInvariant());
            if (!hash.IsNullOrWhiteSpace())
                activationKey.Hash = _encoding.GetBytes(hash.ToUpperInvariant());
            if (!tail.IsNullOrWhiteSpace())
                activationKey.Seed = _encoding.GetBytes(tail.ToUpperInvariant());
        }

        // Parses the string and fill the activation key data.
        internal void InternalParse(ActivationKey activationKey, string input)
        {
            if (input.IsNullOrEmpty())
                return;
            string[] items = input.ToUpperInvariant().Split(_delimiters);
            if (items.Length >= 3)
            {
                activationKey.Data = _encoding.GetBytes(items[0]);
                activationKey.Hash = _encoding.GetBytes(items[1]);
                activationKey.Seed = _encoding.GetBytes(items[2]);
            }

        }

        /// <summary>
        /// Creates an <see cref="ActivationKey.Data"/> instance based on the data, hash and tail, represented as a string.
        /// </summary>
        /// <param name="data">String containing data.</param>
        /// <param name="hash">String containing the hash.</param>
        /// <param name="tail">String containing the tail.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> instance containing the parsed data.</returns>
        public ActivationKey Parse(string data, string hash, string tail)
        {
            ActivationKey activationKey = new ActivationKey();
            InternalParse(activationKey, data, hash, tail);
            return activationKey;
        }

        /// <summary>
        /// Creates an <see cref="ActivationKey.Data"/> instance from a string.
        /// </summary>
        /// <param name="input">A string containing <see cref="ActivationKey.Data"/> text representation.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> containing the parsed data.</returns>
        public ActivationKey Parse(string input)
        {
            ActivationKey activationKey = new ActivationKey();
            InternalParse(activationKey, input);
            return activationKey;
        }

        /// <summary>
        /// Parses the activation string and creates an <see cref="ActivationKey"/> instance.
        /// </summary>
        /// <param name="reader">A <see cref="StreamReader"/> object containing the activation string.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> containing the parsed data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is null.</exception>
        public ActivationKey Parse(StreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            return Parse(reader.ReadToEnd());
        }

        /// <summary>
        /// Parses the stream and creates an <see cref="ActivationKey"/> instance.
        /// </summary>
        /// <param name="stream">A stream containing the activation key text.</param>
        /// <param name="encoding">The encoding used to decode text. If null, UTF-8 is used.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> containing the parsed data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="stream"/> cannot be read or is empty.</exception>
        public ActivationKey Parse(Stream stream, Encoding encoding = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), GetResourceString("ArgumentNull_Stream"));
            if (!stream.CanRead)
                throw new ArgumentException(GetResourceString("Argument_StreamNotReadable"));
            if (stream.Length == 0)
                throw new ArgumentException(GetResourceString("Serialization_Stream"));
            if (encoding == null)
                encoding = Encoding.UTF8;

            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                return Parse(reader);
            }
        }

        /// <summary>
        /// Gets <see cref="ActivationKey.Data"/> property as string.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> for the text view.</param>
        /// <returns>String representing the <see cref="ActivationKey.Data"/> property.</returns>
        public string GetData(ActivationKey activationKey)
        {
            return GetStringSafe(activationKey.Data);
        }

        /// <summary>
        /// Gets <see cref="ActivationKey.Hash"/> property as string.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> for the text view.</param>
        /// <returns>String representing the <see cref="ActivationKey.Hash"/> property.</returns>
        public string GetHash(ActivationKey activationKey)
        {
            return GetStringSafe(activationKey.Hash);
        }

        /// <summary>
        /// Gets <see cref="ActivationKey.Seed"/> property as string.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> for the text view.</param>
        /// <returns>String representing the <see cref="ActivationKey.Seed"/> property.</returns>
        public string GetSeed(ActivationKey activationKey)
        {
            return GetStringSafe(activationKey.Seed);
        }

        /// <summary>
        /// Returns a text representing the <see cref="ActivationKey"/> object.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> for the text view.</param>
        /// <returns>A string containing activation key as a sequence of parts separated by a delimiter.</returns>
        public string GetString(ActivationKey activationKey)
        {
            char sep = _delimiters.Length > 0 ? _delimiters[0] : DefaultDelimiter;
            return activationKey.InvalidState
                ? string.Empty // If the key has no data, hash or tail.
                : $"{GetData(activationKey)}{sep}{GetHash(activationKey)}{sep}{GetSeed(activationKey)}";
        }

        /// <summary>
        /// Returns a part of the <see cref="ActivationKey"/> according to the specified format.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> for the text view.</param>
        /// <param name="format">A string containing format specifier.</param>
        /// <returns>A string containing a part of the <see cref="ActivationKey"/>.</returns>
        /// <exception cref="FormatException">The <paramref name="format"/> parameter contains an invalid specifier.</exception>
        public string GetFormat(ActivationKey activationKey, string format)
        {
            if (format.IsNullOrWhiteSpace())
                return GetString(activationKey);

            switch (format.ToUpperInvariant())
            {
                case "D":
                    return GetData(activationKey);
                case "H":
                    return GetHash(activationKey);
                case "S":
                    return GetSeed(activationKey);
                default:
                    throw new FormatException(GetResourceString("Format_BadFormatSpecifier"));
            }
        }

        /// <summary>
        /// Replaces certain characters in the template with the corresponding parts of the <see cref="ActivationKey"/>.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> for the text view.</param>
        /// <param name="pattern">A string containing specific marks for inserting parts of the <see cref="ActivationKey"/>.</param>
        /// <returns>A string obtained from a template in which all tags are replaced
        /// with the corresponding parts of the <see cref="ActivationKey"/>.</returns>
        public string GetPattern(ActivationKey activationKey, string pattern)
        {
            if (pattern.IsNullOrEmpty() || activationKey.InvalidState)
                return string.Empty;

            StringBuilder cache = new StringBuilder(pattern)
                .Replace("%D", GetData(activationKey))
                .Replace("%H", GetHash(activationKey))
                .Replace("%S", GetSeed(activationKey));
            return cache.ToString();
        }

        /// <summary>
        /// Writes key activation data to the stream.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> to serialize.</param>
        /// <param name="writer">Serialization stream writer.</param>
        public void Write(ActivationKey activationKey, StreamWriter writer)
        {
            writer.Write(GetString(activationKey));
        }

        /// <summary>
        /// Writes key activation data to the stream.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> to serialize.</param>
        /// <param name="stream">Serialization stream.</param>
        public void Write(ActivationKey activationKey, Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                Write(activationKey, writer);
            }
        }
    }
}