/***************************************************************

•   File: ActivationKeyBinaryParser.cs

•   Description.

    ActivationKeyBinaryParser   is a  parser    of  binary  data
    containing activation keys, which   can be represented  as a
    sequence of bytes  with  the  required  header.

    The class provides  methods  for parsing activation  keys as
    well  as  for creating instances of the ActivationKey class,
    which is a data  structure  containing information about the
    activation key.

***************************************************************/

using System.IO;
using static System.InternalTools;

namespace System.Security.Activation
{
    /// <summary>
    /// Represents a parser that provides tools for analyzing binary data with activation keys
    /// which can be represented as bytes sequence with required header.
    /// </summary>
    public class ActivationKeyBinaryParser : ICloneable
    {
        // A constant representing the default header "aK" of the activation key file. You can replace it with your own value.
        internal const ushort BinaryHeader = 0x4B61;

        internal static readonly ActivationKeyBinaryParser InternalParser = new ActivationKeyBinaryParser();
        private ushort _header = BinaryHeader;

        /// <summary>
        /// The header of the activation key binary file, which is used to verify the file format.
        /// </summary>
        public ushort Header 
        { 
            get => _header;
            internal set => _header = value;
        }

        /// <summary>
        /// Returns the default instance of the <see cref="ActivationKeyBinaryParser"/>.
        /// </summary>
        public static ActivationKeyBinaryParser DefaultParser => (ActivationKeyBinaryParser) InternalParser.Clone();

        /// <summary>
        /// Initializes a new instance of <see cref="ActivationKeyTextParser"/> using the default parameters.
        /// </summary>
        public ActivationKeyBinaryParser()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ActivationKeyTextParser"/> using the specified parameters.
        /// <param name="header">The header of the activation key file, which is used to verify the file format.</param>
        /// </summary>
        public ActivationKeyBinaryParser(ushort header)
        {
            _header = header;
        }

        // Parses the stream and fill the activation key data.
        internal void InternalParse(ActivationKey activationKey, BinaryReader reader)
        {
            Stream stream = reader.BaseStream;
            try
            {
                ushort header = reader.ReadUInt16();
                if (header != _header)
                    throw new InvalidDataException(GetResourceString("Serialization_BinaryHeader", stream));
                int dataLength = reader.ReadInt32();
                int hashLength = reader.ReadInt32();
                int tailLength = reader.ReadInt32();
                activationKey.Data = reader.ReadBytes(dataLength);
                activationKey.Hash = reader.ReadBytes(hashLength);
                activationKey.Seed = reader.ReadBytes(tailLength);
            }
            catch (Exception e)
            {
                throw new InvalidDataException(GetResourceString("Serialization_InvalidFormat", stream.Position), e);
            }
        }

        // Parses the activation key binary file and populates the ActivationKey object with data from the file.
        internal void InternalParse(ActivationKey activationKey, byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                InternalParse(activationKey, reader);
            }
        }

        /// <summary>
        /// Parses key activation data from a reader.
        /// </summary>
        /// <param name="reader">A reader from the stream containing key activation data.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> instance containing the parsed data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is null.</exception>
        public ActivationKey Parse(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            ActivationKey activationKey = new ActivationKey();
            InternalParse(activationKey, reader);
            return activationKey;
        }

        /// <summary>
        /// Parses key activation data from a stream.
        /// </summary>
        /// <param name="stream">A stream containing key activation data.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> instance containing the parsed data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the stream is not readable or contains no data.</exception>
        public ActivationKey Parse(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), GetResourceString("ArgumentNull_Stream"));
            if (!stream.CanRead)
                throw new ArgumentException(GetResourceString("Argument_StreamNotReadable"));
            if (stream.Length == 0)
                throw new ArgumentException(GetResourceString("Serialization_Stream"));


            using (BinaryReader reader = new BinaryReader(stream))
            {
                return Parse(reader);
            }
        }

        /// <summary>
        /// Parses key activation data from a byte array.
        /// </summary>
        /// <param name="bytes">A byte array containing key activation data.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> instance containing the parsed data.</returns>
        /// <exception cref="ArgumentException">Thrown if array is null or empty.</exception>
        public ActivationKey Parse(byte[] bytes)
        {
            if (bytes.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullArray"), nameof(bytes));

            ActivationKey activationKey = new ActivationKey();
            InternalParse(activationKey, bytes);
            return activationKey;
        }

        // Returns the length of the data that will be written to the stream when the ActivationKey object is serialized.
        private int GetLength(ActivationKey activationKey)
        {
            return sizeof(ushort) 
                   + sizeof(int) * 3 
                    + activationKey.Data.Length 
                    + activationKey.Hash.Length 
                    + activationKey.Seed.Length;
        }

        /// <summary>
        /// Writes key activation data to the stream.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> to serialize.</param>
        /// <param name="writer">Serialization stream writer.</param>
        /// <exception cref="ArgumentNullException">Thrown if one of arguments is null.</exception>
        public void Write(ActivationKey activationKey, BinaryWriter writer)
        {
            if (activationKey == null)
                throw new ArgumentNullException(nameof(activationKey), GetResourceString("ArgumentNull_WithParamName", nameof(activationKey)));
            if (writer == null)
                throw new ArgumentNullException(nameof(writer), GetResourceString("ArgumentNull_WithParamName", nameof(writer)));

            writer.Write(BinaryHeader);
            writer.Write(activationKey.Data.Length);
            writer.Write(activationKey.Hash.Length);
            writer.Write(activationKey.Seed.Length);
            writer.Write(activationKey.Data);
            writer.Write(activationKey.Hash);
            writer.Write(activationKey.Seed);
        }

        /// <summary>
        /// Writes key activation data to the stream.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> to serialize.</param>
        /// <param name="stream">Serialization stream.</param>
        /// <exception cref="ArgumentNullException">Thrown if one of arguments is null.</exception>
        /// <exception cref="ArgumentException">Thrown if output stream is not writable.</exception>
        public void Write(ActivationKey activationKey, Stream stream)
        {
            if (activationKey == null)
                throw new ArgumentNullException(nameof(activationKey), GetResourceString("ArgumentNull_WithParamName", nameof(activationKey)));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), GetResourceString("ArgumentNull_Stream"));
            if (!stream.CanWrite)
                throw new ArgumentException(GetResourceString("Argument_StreamNotWritable"));


            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                Write(activationKey, writer);
            }
        }

        /// <summary>
        /// Converts the <see cref="ActivationKey"/> object to a byte array.
        /// </summary>
        /// <param name="activationKey">An instance of <see cref="ActivationKey"/> to convert.</param>
        /// <returns>Byte array representing the <see cref="ActivationKey"/> object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if activation key is null.</exception>
        public byte[] GetBytes(ActivationKey activationKey)
        {
            if (activationKey == null)
                throw new ArgumentNullException(nameof(activationKey), GetResourceString("ArgumentNull_WithParamName", nameof(activationKey)));
            if (activationKey.InvalidState)
                throw new InvalidOperationException("Arg_InvalidOperationException");

            using (MemoryStream stream = new MemoryStream(GetLength(activationKey)))
            {
                Write(activationKey, stream);
                return stream.ToArray();
            }
        }

        /// <inheritdoc />
        public object Clone()
        {
            return new ActivationKeyBinaryParser(_header);
        }
    }
}