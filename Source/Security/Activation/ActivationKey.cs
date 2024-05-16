/***************************************************************

•   File: ActivationKey.cs

•   Description.

    ActivationKey  is  a  class    that  represents   a software
    activation  key  containing   encrypted data, a  checksum to
    verify  the key, and   an   initialization  vector   used to
    increase  the reliability   and uniqueness of  the generated
    keys.

    The  ActivationKey  class  can be converted to readable text
    for delivery to the end user, or  saved  in  binary  or text
    format to a file.

        Data — encrypted data  that can be recovered.
        Hash - checksum   for  verifying   the key.
        Seed — initialization vector  is used to ensure security
               when encrypting and decrypting data.

    The activation  key can be  converted into readable text for
    delivery  to the end user or  saved  in  binary format  to a
    file.

    The  class also contains   tools  for  reading   and writing
    activation  key  files,  creating   and verifying keys using
    various encryption and hashing algorithms,  as  well  as for
    extracting   data  from  the    encrypted part of   the key.

    Overall,  the ActivationKey  class  is a convenient tool for
    creating, validating,  and managing activation keys that are
    used to protect software and other products.

***************************************************************/

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using static System.InternalTools;

namespace System.Security.Activation
{
    /// <summary>
    /// Represents the activation key for the specified encoding.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ActivationKeyConverter))]
    [DebuggerDisplay("{ToString()}")]
    [StructLayout(LayoutKind.Sequential)]
    public sealed class ActivationKey : ISerializable, IXmlSerializable, IFormattable, ICloneable, IDisposable
    {
        #region Properties

        private byte[] _data;
        private byte[] _hash;
        private byte[] _seed;
        private static ActivationKeyTextParser InternalTextParser => ActivationKeyTextParser.DefaultParser;
        private static ActivationKeyBinaryParser InternalBinaryParser => ActivationKeyBinaryParser.DefaultParser;

        /// <summary>
        /// Returns an activation key file manager with default parameters. Used to loading and saving activation keys.
        /// </summary>
        public static ActivationKeyManager DefaultManager => ActivationKeyManager.DefaultManager;

        /// <summary>
        /// Returns an activation key encryptor with default encryption algorithm. Used to generating activation keys.
        /// </summary>
        public static ActivationKeyEncryptor DefaultEncryptor => ActivationKeyEncryptor.DefaultEncryptor;

        /// <summary>
        /// Password-encrypted part of the activation key. Contains expiration date of key and stored options.
        /// </summary>
        public byte[] Data
        {
            get => _data?.ArrayClone();
            internal set => _data = value;
        }

        /// <summary>
        /// Checksum of all activation parameters. Ensures that the key is valid during verification.
        /// </summary>
        public byte[] Hash
        {
            get => _hash?.ArrayClone();
            internal set => _hash = value;
        }

        /// <summary>
        /// The value used to initialize encryption providers.
        /// </summary>
        public byte[] Seed
        {
            get => _seed?.ArrayClone();
            internal set => _seed = value;
        }

        /// <summary>
        /// Returns <see langword="true" /> if this instance of <see cref = "ActivationKey" />
        /// contains complete data, hash, seed and is ready to be verified.
        /// </summary>
        [field: NonSerialized]
        public bool Ready
        {
            get => !InvalidState;
        }

        // True if not ready
        [field: NonSerialized]
        internal bool InvalidState
        {
            get => Data.IsNullOrEmpty() || Hash.IsNullOrEmpty() || Seed.IsNullOrEmpty();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />.
        /// </summary> 
        public ActivationKey()
        {
        }

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" /> using serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> for retrieving data.</param>
        /// <param name="context">A <see cref="StreamingContext"/> object containing information
        /// about the serialization context.</param>
        /// <exception cref="ArgumentNullException">Occurs if the info parameter is null.</exception>
        private ActivationKey(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            Data = (byte[]) info.GetValue(nameof(Data), typeof(byte[]));
            Hash = (byte[]) info.GetValue(nameof(Hash), typeof(byte[]));
            Seed = (byte[]) info.GetValue(nameof(Seed), typeof(byte[]));
        }

        /// <summary>
        /// Creates an instance of the <see cref = "ActivationKey" /> using the provided byte array.
        /// </summary>
        /// <param name="activationKey">Byte array contains data of the activation key.</param>
        public ActivationKey(byte[] activationKey)
        {
            if (activationKey.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullArray"), nameof(activationKey));

            InternalBinaryParser.InternalParse(this, activationKey);
        }

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using the constituent parts of the previously generated key.
        /// </summary>
        /// <param name = "data"> Encrypted part of the key. </param>
        /// <param name = "hash"> Key checksum. </param>
        /// <param name = "seed"> Key initialization vector. </param>
        /// <exception cref="ArgumentException">One of the arguments is null or empty.</exception>
        public ActivationKey(byte[] data, byte[] hash, byte[] seed)
        {
            if (data.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullArray"), nameof(data));
            if (hash.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullArray"), nameof(hash));
            if (seed.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullArray"), nameof(seed));

            Data = data;
            Hash = hash;
            Seed = seed;
        }

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using textual representations of the constituent parts of the previously generated key.
        /// </summary>
        /// <param name = "data"> Encrypted part of the key. </param>
        /// <param name = "hash"> Key checksum. </param>
        /// <param name = "seed"> Key initialization vector. </param> 
        /// <param name="encoding">Data encoding.</param>
        /// <exception cref="ArgumentException">One of the arguments is null or empty.</exception>
        public ActivationKey(string data, string hash, string seed, IPrintableEncoding encoding = null)
        {
            if (data.IsNullOrWhiteSpace())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(data));
            if (hash.IsNullOrWhiteSpace())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(hash));
            if (seed.IsNullOrWhiteSpace())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(seed));

            ActivationKeyTextParser parser = new ActivationKeyTextParser(encoding);
            parser.InternalParse(this, data, hash, seed);
        }


        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using the text representation of the previously generated key.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param>
        /// <exception cref="ArgumentException">Argument <paramref name="activationKey"/> is null or empty.</exception>
        public ActivationKey(string activationKey)
        {
            if (activationKey.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(activationKey));

            InternalTextParser.InternalParse(this, activationKey);
        }

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using the text representation of the previously generated key.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param>
        /// <param name="delimiters">Characters used as key part delimiters.</param>
        /// <exception cref="ArgumentException">Argument <paramref name="activationKey"/> is null or empty.</exception>
        public ActivationKey(string activationKey, params char[] delimiters)
        {
            if (activationKey.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(activationKey));

            ActivationKeyTextParser parser = new ActivationKeyTextParser(delimiters);
            parser.InternalParse(this, activationKey);
        }

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using the text representation of the previously generated key.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param>
        /// <param name="encoding">Data encoding.</param>
        /// <param name="delimiters">Characters used as key part delimiters.</param>
        /// <exception cref="ArgumentException">Argument <paramref name="activationKey"/> is null or empty.</exception>
        public ActivationKey(string activationKey, IPrintableEncoding encoding, params char[] delimiters)
        {
            if (activationKey.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(activationKey));

            ActivationKeyTextParser parser = new ActivationKeyTextParser(encoding, delimiters);
            parser.InternalParse(this, activationKey);
        }
        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using the text representation of the previously generated key.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param>
        /// <param name="encoding">Data encoding.</param>
        /// <param name="delimiters">Characters used as key part delimiters.</param>
        /// <exception cref="ArgumentException">Argument <paramref name="activationKey"/> is null or empty.</exception>
        public ActivationKey(string activationKey, PrintableEncoding encoding, params char[] delimiters)
        {
            if (activationKey.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(activationKey));

            ActivationKeyTextParser parser = new ActivationKeyTextParser(encoding, delimiters);
            parser.InternalParse(this, activationKey);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Converts <see cref="ActivationKey"/> instance to string.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param> 
        public static explicit operator string(ActivationKey activationKey)
        {
            return activationKey.ToString();
        }

        /// <summary>
        /// Converts the string containing the activation key to an instance of <see cref="ActivationKey"/>.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param> 
        public static explicit operator ActivationKey(string activationKey)
        {
            return new ActivationKey(activationKey);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks the activation key.
        /// </summary>
        /// <param name="environment">Software-defined environmental settings required to validate the activation key.</param>
        /// <returns>True if the activation key is valid, false otherwise.</returns>
        public bool Verify(params object[] environment)
        {
            using (ActivationKeyDecryptor decryptor = CreateDecryptor(this, environment))
            {
                return decryptor.Success;
            }
        }

        /// <summary>
        /// Checks the activation key.
        /// </summary>
        /// <typeparam name="TSymmetricAlgorithm">Generic SymmetricAlgorithm type that will be used to decrypt data.
        /// This type must be an inheritor of the <see cref="SymmetricAlgorithm"/> class.
        /// </typeparam>
        /// <typeparam name="THashAlgorithm">The generic HashAlgorithm type that will be used to calculate the hash of the data.
        /// This type must be an inheritor of the <see cref="HashAlgorithm"/> class.
        /// </typeparam>
        /// <param name="environment">Software-defined environmental settings required to validate the activation key.</param>
        /// <returns>True if the activation key is valid, false otherwise.</returns>
        public bool Verify<TSymmetricAlgorithm, THashAlgorithm>(params object[] environment)
            where TSymmetricAlgorithm : SymmetricAlgorithm
            where THashAlgorithm : HashAlgorithm
        {
            using (ActivationKeyDecryptor decryptor = CreateDecryptor<TSymmetricAlgorithm, THashAlgorithm>(this, environment))
            {
                return decryptor.Success;
            }
        }

        /// <inheritdoc cref="ICloneable.Clone()"/>
        public object Clone()
        {
            return new ActivationKey()
            {
                Data = Data.ArrayClone(),
                Hash = Hash.ArrayClone(),
                Seed = Seed.ArrayClone()
            }; ;
        }

        /// <summary>
        /// Converts the data into binary format for saving to a file.
        /// </summary>
        /// <returns>An array containing the binary representation of the data.</returns>
        public byte[] ToBinary()
        {
            return InternalBinaryParser.GetBytes(this);
        }

        /// <summary>
        /// Converts the data into binary format for saving to a file.
        /// </summary>
        /// <param name="header">The first 2 bytes header.</param>
        /// <returns>An array containing the binary representation of the data.</returns>
        public byte[] ToBinary(ushort header)
        {
            return new ActivationKeyBinaryParser(header).GetBytes(this);
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return InternalTextParser.GetString(this);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <param name="encoding">Data encoding.</param>
        /// <returns>A string that represents the current object.</returns>
        public string ToString(IPrintableEncoding encoding)
        {
            return new ActivationKeyTextParser(encoding).GetString(this);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <param name="encoding">Data encoding.</param>
        /// <returns>A string that represents the current object.</returns>
        public string ToString(PrintableEncoding encoding)
        {
            return new ActivationKeyTextParser(encoding).GetString(this);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <param name="delimiter">A character used as a key part delimiter.</param>
        /// <param name="encoding">Data encoding.</param>
        /// <returns>A string that represents the current object.</returns>
        public string ToString(char delimiter, IPrintableEncoding encoding = null)
        {
            return new ActivationKeyTextParser(encoding, delimiter).GetString(this);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <param name="delimiter">A character used as a key part delimiter.</param>
        /// <param name="encoding">Data encoding.</param>
        /// <returns>A string that represents the current object.</returns>
        public string ToString(char delimiter, PrintableEncoding encoding)
        {
            return new ActivationKeyTextParser(encoding, delimiter).GetString(this);
        }

        /// <summary>
        /// Converts the current instance of <see cref="ActivationKey"/> to a string in the specified format.
        /// Replaces the characters %D, %H and %T in the string specified in <paramref name="pattern"/> 
        /// with <see cref="Data"/>, <see cref="Hash"/> and <see cref= "Seed"/> properties 
        /// of the current instance of <see cref="ActivationKey"/>.
        /// </summary>
        /// <param name="pattern">A string containing formatting information.</param>
        /// <param name="encoding">Data encoding.</param>
        /// <returns>The string value of the current instance of <see cref="ActivationKey"/> in the specified format.</returns>
        /// <exception cref="ArgumentException">Argument <paramref name="pattern"/> is null or empty.</exception>
        public string ToString(string pattern, IPrintableEncoding encoding = null)
        {
            if (pattern.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(pattern));

            return new ActivationKeyTextParser(encoding).GetPattern(this, pattern);
        }

        /// <summary>
        /// Converts the current instance of <see cref="ActivationKey"/> to a string in the specified format.
        /// Replaces the characters %D, %H and %T in the string specified in <paramref name="pattern"/> 
        /// with <see cref="Data"/>, <see cref="Hash"/> and <see cref= "Seed"/> properties 
        /// of the current instance of <see cref="ActivationKey"/>.
        /// </summary>
        /// <param name="pattern">A string containing formatting information.</param>
        /// <param name="encoding">Data encoding.</param>
        /// <returns>The string value of the current instance of <see cref="ActivationKey"/> in the specified format.</returns>
        /// <exception cref="ArgumentException">Argument <paramref name="pattern"/> is null or empty.</exception>
        public string ToString(string pattern, PrintableEncoding encoding)
        {
            if (pattern.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(pattern));

            return new ActivationKeyTextParser(encoding).GetPattern(this, pattern);
        }

        /// <inheritdoc cref="IFormattable.ToString(string, IFormatProvider)"/>
        /// <exception cref="FormatException">Format specifier is unsupported.</exception>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return InternalTextParser.GetFormat(this, format);
        }

        /// <inheritdoc cref="object.GetHashCode" />
        public override int GetHashCode()
        {
            return SipHashAlgorithm.DefaultHash.GetInt32(Serialize(Data, Hash, Seed));
        }

        /// <inheritdoc cref="ISerializable.GetObjectData(SerializationInfo, StreamingContext)"/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (InvalidState)
                throw new InvalidOperationException(GetResourceString("Arg_InvalidOperationException"));

            info.AddValue(nameof(Data), Data);
            info.AddValue(nameof(Hash), Hash);
            info.AddValue(nameof(Seed), Seed);
        }

        /// <inheritdoc cref="IXmlSerializable.GetSchema()"/>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <inheritdoc cref="IXmlSerializable.ReadXml(XmlReader)"/>
        /// <exception cref="ArgumentNullException">Argument <paramref name="reader"/> is null or empty.</exception>
        public void ReadXml(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(null, GetResourceString("ArgumentNull_WithParamName", nameof(reader)));

            InternalTextParser.InternalParse(this, reader.ReadContentAsString());
        }

        /// <inheritdoc cref="IXmlSerializable.WriteXml(XmlWriter)"/>
        /// <exception cref="ArgumentNullException">Argument <paramref name="writer"/> is null or empty.</exception>
        public void WriteXml(XmlWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(null, GetResourceString("ArgumentNull_WithParamName", nameof(writer)));

            writer.WriteString(ToString());
        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            Data.Clear();
            Hash.Clear();
            Seed.Clear();

            Data = null;
            Hash = null;
            Seed = null;
        }

        /// <summary>
        /// Returns an activation key file manager using the specified parameters. Used to loading and saving activation keys.
        /// </summary>
        /// <param name="header">The header of the binary activation key data, which is used to verify the file format.</param>
        /// <param name="encoding">Encoding used to encode/decode activation key data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public static ActivationKeyManager CreateManager(ushort header, IPrintableEncoding encoding, params char[] delimiters)
        {
            return new ActivationKeyManager(header, encoding, delimiters);
        }

        /// <summary>
        /// Returns an activation key file manager using the specified parameters. Used to loading and saving activation keys.
        /// </summary>
        /// <param name="encoding">Encoding used to encode/decode activation key data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public static ActivationKeyManager CreateManager(IPrintableEncoding encoding, params char[] delimiters)
        {
            return new ActivationKeyManager(encoding, delimiters);
        }

        /// <summary>
        /// Returns an activation key file manager using the specified parameters. Used to loading and saving activation keys.
        /// </summary>
        /// <param name="header">The header of the binary activation key data, which is used to verify the file format.</param>
        /// <param name="encoding">Encoding used to encode/decode activation key data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public static ActivationKeyManager CreateManager(ushort header, PrintableEncoding encoding, params char[] delimiters)
        {
            return new ActivationKeyManager(header, encoding, delimiters);
        }

        /// <summary>
        /// Returns an activation key file manager using the specified parameters. Used to loading and saving activation keys.
        /// </summary>
        /// <param name="encoding">Encoding used to encode/decode activation key data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public static ActivationKeyManager CreateManager(PrintableEncoding encoding, params char[] delimiters)
        {
            return new ActivationKeyManager(encoding, delimiters);
        }

        /// <summary>
        /// Returns an activation key file manager using the specified parameters. Used to loading and saving activation keys.
        /// </summary>
        /// <param name="header">The header of the binary activation key data, which is used to verify the file format.</param>
        public static ActivationKeyManager CreateManager(ushort header)
        {
            return new ActivationKeyManager(header);
        }

        /// <summary>
        /// Creates an encryptor that can generate new activation keys
        /// bound to the specified environment parameters using default cryptographic algorithms.
        /// </summary>
        /// <param name="environment">Environment parameters that will be used to generate the activation key.
        /// These parameters can be used to create a unique activation key that can be used to authenticate a user or device.
        /// </param>
        /// <returns>An instance of the <see cref="ActivationKeyEncryptor"/>.</returns>
        public static ActivationKeyEncryptor CreateEncryptor(params object[] environment)
        {
            return new ActivationKeyEncryptor(environment);
        }

        /// <summary>
        /// Creates an encryptor that can generate new activation keys
        /// bound to the specified environment parameters using specified cryptographic algorithms.
        /// </summary>
        /// <typeparam name="TSymmetricAlgorithm">Generic SymmetricAlgorithm type that will be used to encrypt data.
        /// This type must be an inheritor of the <see cref="SymmetricAlgorithm"/> class.
        /// </typeparam>
        /// <typeparam name="THashAlgorithm">The generic HashAlgorithm type that will be used to calculate the hash of the data.
        /// This type must be an inheritor of the <see cref="HashAlgorithm"/> class.
        /// </typeparam>
        /// <param name="environment">Environment parameters that will be used to generate the activation key.
        /// These parameters can be used to create a unique activation key that can be used to authenticate a user or device.
        /// </param>
        /// <returns>An instance of the <see cref="ActivationKeyEncryptor"/>.</returns>
        public static ActivationKeyEncryptor CreateEncryptor<TSymmetricAlgorithm, THashAlgorithm>(
            params object[] environment)
            where TSymmetricAlgorithm : SymmetricAlgorithm
            where THashAlgorithm : HashAlgorithm
        {
            SymmetricAlgorithm cryptoAlg = Activator.CreateInstance<TSymmetricAlgorithm>();
            HashAlgorithm hashAlg = Activator.CreateInstance<THashAlgorithm>();

            return new ActivationKeyEncryptor(cryptoAlg, hashAlg, environment);

        }

        /// <summary>
        /// Set the default encoding to encode/decode the activation key.
        /// </summary>
        /// <param name="encoding">Encoding that will be used by default.</param>
        public static void SetDefaultEncoding(IPrintableEncoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding),
                    GetResourceString("ArgumentNull_WithParamName", nameof(encoding)));
            ActivationKeyTextParser.InternalParser.Encoding = encoding;
        }

        /// <summary>
        /// Set the default encoding to encode/decode the activation key.
        /// </summary>
        /// <param name="encoding">Encoding that will be used by default.</param>
        public static void SetDefaultEncoding(PrintableEncoding encoding)
        {
            ActivationKeyTextParser.InternalParser.Encoding = ActivationKeyTextParser.GetEncoding(encoding);
        }

        /// <summary>
        /// Set the default encoding to encode/decode the activation key using the following alphabet.
        /// </summary>
        /// <param name="alphabet">String containing characters that will be used by default.</param>
        public static void SetDefaultEncoding(string alphabet)
        {
            ActivationKeyTextParser.InternalParser.Encoding = ActivationKeyTextParser.GetEncoding(alphabet);
        }

        /// <summary>
        /// Set default characters that are used to split the activation key into parts.
        /// </summary>
        /// <param name="delimiters">Characters that are used by default.</param>
        public static void SetDefaultDelimiters(params char[] delimiters)
        {
            if (delimiters.IsNullOrEmpty())
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullArray", nameof(delimiters)));
            ActivationKeyTextParser.InternalParser.Delimiters = delimiters;
        }

        /// <summary>
        /// Set default header that will be used to validate the binary format of the activation key.
        /// </summary>
        /// <param name="header">Value that will be used as binary header by default.</param>
        public static void SetDefaultBinaryHeader(ushort header)
        {
            ActivationKeyBinaryParser.InternalParser.Header = header;
        }

        /// <summary>
        /// Returns a decryptor bound to specified environment parameters using default cryptographic algorithms.
        /// Used to verify the current activation key and extract encrypted data.
        /// </summary>
        /// <param name="environment">Environment binding parameters used to generate the key.
        /// These parameters can be used to create a unique activation key that can be used to authenticate a user or device.</param>
        /// <returns>An instance of the <see cref="ActivationKeyDecryptor"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <see cref="ActivationKey.Ready"/> property for this instance of <see cref="ActivationKey"/> is false.
        /// </exception>
        public ActivationKeyDecryptor CreateDecryptor(params object[] environment)
        {
            if (InvalidState)
                throw new InvalidOperationException(GetResourceString("Arg_InvalidOperationException"));

            return new ActivationKeyDecryptor(this, environment);
        }

        /// <summary>
        /// Returns a decryptor bound to specified environment parameters using the specified encryption and hashing algorithms.
        /// Used to verify the current activation key and extract encrypted data.
        /// </summary>
        /// <typeparam name="TSymmetricAlgorithm">Generic SymmetricAlgorithm type that will be used to decrypt data.
        /// This type must be an inheritor of the <see cref="SymmetricAlgorithm"/> class.
        /// </typeparam>
        /// <typeparam name="THashAlgorithm">The generic HashAlgorithm type that will be used to calculate the hash of the data.
        /// This type must be an inheritor of the <see cref="HashAlgorithm"/> class.
        /// </typeparam>
        /// <param name="environment">Environment binding parameters used to generate the key.
        /// These parameters can be used to create a unique activation key that can be used to authenticate a user or device.</param>
        /// <returns>An instance of the <see cref="ActivationKeyDecryptor"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <see cref="ActivationKey.Ready"/> property for this instance of <see cref="ActivationKey"/> is false.
        /// </exception>
        public ActivationKeyDecryptor CreateDecryptor<TSymmetricAlgorithm, THashAlgorithm>(params object[] environment)
            where TSymmetricAlgorithm : SymmetricAlgorithm
            where THashAlgorithm : HashAlgorithm
        {
            if (InvalidState)
                throw new InvalidOperationException(GetResourceString("Arg_InvalidOperationException"));

            using (SymmetricAlgorithm cryptoAlg = Activator.CreateInstance<TSymmetricAlgorithm>())
            using (HashAlgorithm hashAlg = Activator.CreateInstance<THashAlgorithm>())
            {
                return new ActivationKeyDecryptor(cryptoAlg, hashAlg, this, environment);
            }
        }

        #endregion
    }
}