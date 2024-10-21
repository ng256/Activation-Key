/*************************************************************************************************************
System.Security.Cryptography.ActivationKey v 1.2
Represents the activation key used to protect the licensed application.
Contains methods for updating the key based on the specified hardware and software environment.

Key format: DATA-HASH-SEED. For Example, KCATBZ14Y4UEA-VGDM2ZQ-ATSVYMI.

Data 	A part of the key encrypted with an environment. Contains the key expiration date and application data.
Hash 	Checksum of the key expiration date, data, seed value and environment parameters.
Seed	Initialization value that used to encode/decode the data.

Distributed under MIT license (https://mit-license.org/)
© 2021-2024 Pavel Bashkardin (https://github.com/ng256)
**************************************************************************************************************/

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Security.Cryptography
{
    /// <summary>
    /// Represents the activation key used to protect the licensed application.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ActivationKeyConverter))]
    [DebuggerDisplay("{ToString()}")]
    public sealed class ActivationKey : IFormattable, ICloneable, IDisposable, IXmlSerializable
    {
        #region Static members
        private const int IVSIZE = 4; // Default length of the initialization vector.
        private static ResourceSet _mscorlib = null; // Mscorlib resources.
        private static RNGCryptoServiceProvider _rng = null;
        private static RNGCryptoServiceProvider InternalRng => _rng ?? (_rng = new RNGCryptoServiceProvider());
        #endregion

        #region Properties
        /// <summary>
        /// Encrypted part of the activation key. Contains expiration date of key and stored data.
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Checksum of key expiration date, data and environment parameters.
        /// </summary>
        public byte[] Hash
        {
            get;
            private set;
        }

        /// <summary>
        /// Initialization value.
        /// </summary>
        public byte[] Seed
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns <see langword="true" /> if this instance of <see cref = "ActivationKey" /> contains complete data and is ready to be verified.
        /// </summary>
        public bool Ready
        {
            get => !InvalidState;
        }

        // True if not ready
        private bool InvalidState
        {
            get => IsNullOrEmpty(Data) || IsNullOrEmpty(Hash) || IsNullOrEmpty(Seed);
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
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "expirationDate"> The expiration date of this key. </param>
        /// <param name = "data">
        ///  Additional data that determine the capabilities of the program, for example, the number
        ///  launches or restriction on the use of certain functions. </param>
        /// <param name = "environment">
        ///  Additional parameters that define the firmware binding,
        ///  such as workstation id, application name, etc.
        /// </param> 
        public ActivationKey(DateTime expirationDate, object data = null, params object[] environment)
        {
            byte[] iv = new byte[IVSIZE];
            byte[] key = Serialize(environment);
            InternalRng.GetBytes(iv);
            using (_ARC4 arc4 = new _ARC4(key, iv))
            {
                expirationDate = expirationDate.Date;
                long expirationDateStamp = expirationDate.ToBinary();
                Data = arc4.Cipher(expirationDateStamp, data);
                _SMHasher mmh3 = new _SMHasher();
                Hash = mmh3.GetBytes(expirationDateStamp, data, environment, iv);
                Seed = iv;
            }
        }

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "data">
        /// Additional data that determine the capabilities of the program, for example, the number
        /// launches or restriction on the use of certain functions. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param> 
        public ActivationKey(object data = null, params object[] environment)
            : this(DateTime.MaxValue, Serialize(environment), data, environment)
        {
        }

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified date without environment parameters.
        /// </summary>
        /// <param name = "expirationDate"> The expiration date of this key. </param> 
        public ActivationKey(DateTime expirationDate)
            : this(expirationDate, null, null, null)
        {
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
            if (IsNullOrEmpty(data)) throw new ArgumentException(GetResourceString("Arg_EmptyOrNullArray"), nameof(data));
            if (IsNullOrEmpty(hash)) throw new ArgumentException(GetResourceString("Arg_EmptyOrNullArray"), nameof(hash));
            if (IsNullOrEmpty(seed)) throw new ArgumentException(GetResourceString("Arg_EmptyOrNullArray"), nameof(seed));

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
        /// <exception cref="ArgumentException">One of the arguments is null or empty.</exception>
        public ActivationKey(string data, string hash, string seed)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(data));
            if (string.IsNullOrEmpty(hash)) throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(hash));
            if (string.IsNullOrEmpty(seed)) throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(seed));

            using (_Base32 base32 = new _Base32())
            {
                Data = base32.Decode(data.ToUpperInvariant());
                Hash = base32.Decode(hash.ToUpperInvariant());
                Seed = base32.Decode(seed.ToUpperInvariant());
            }
        }

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using the text representation of the previously generated key.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param> 
        /// <exception cref="ArgumentException">Argument <paramref name="activationKey"/> is null or empty.</exception>
        public ActivationKey(string activationKey)
        {
            if (string.IsNullOrEmpty(activationKey)) throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(activationKey));
            InternalParse(activationKey.ToUpperInvariant());
        }

        #endregion

        #region Verifying

        /// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters and returns the decrypted application data.
        /// </summary>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <returns> Decrypted data <see langword = "byte []" />,
        /// or <see langword = "null" /> if the activation key is invalid. </returns> 
        public byte[] GetData(params object[] environment)
        {
            if (InvalidState) return null;

            try
            {
                using (_ARC4 arc4 = new _ARC4(Serialize(environment), Seed))
                {
                    byte[] decryptedData = arc4.Cipher(Data);
                    int dataLength = decryptedData.Length - 8;
                    if (dataLength < 0)
                    {
                        return null;
                    }
                    long expirationDateStamp = BitConverter.ToInt64(decryptedData, 0);
                    DateTime expirationDate = DateTime.FromBinary(expirationDateStamp);
                    if (expirationDate < DateTime.Today)
                    {
                        return null;
                    }
                    byte[] outputData;
                    if (dataLength > 0)
                    {
                        outputData = new byte[dataLength];
                        Buffer.BlockCopy(decryptedData, 8, outputData, 0, dataLength);
                    }
                    else
                    {
                        outputData = new byte[0];
                    }
                    _SMHasher mmh3 = new _SMHasher();
                    byte[] hash = mmh3.GetBytes(expirationDateStamp, decryptedData, environment, Seed);
                    return ByteArrayEquals(Hash, hash) ? outputData : null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if the activation key is valid,
        /// represented by string <paramref name="activationKey"/>
        /// for the specified environment parameters and returns the decrypted application data.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param> 
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <returns> Decrypted data <see langword = "byte []" />,
        /// or <see langword = "null" /> if the activation key is invalid. </returns>
        /// <exception cref="ArgumentException">Argument <paramref name="activationKey"/> is null or empty.</exception>
        public static byte[] GetData(string activationKey, params object[] environment)
        {
            using (ActivationKey key = new ActivationKey(activationKey)) return key.GetData(environment);
        }

        /// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <returns> <see langword = "true" /> if the activation key is valid. </returns> 
        public bool Verify(params object[] environment)
        {
            try
            {
                return GetData(environment) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the activation key is valid,
        /// represented by string <paramref name="activationKey"/>
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param> 
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <returns> <see langword = "true" /> if the activation key is valid. </returns> 
        /// <exception cref="ArgumentException">Argument <paramref name="activationKey"/> is null or empty.</exception>
        public static bool Verify(string activationKey, params object[] environment)
        {
            using (ActivationKey key = new ActivationKey(activationKey)) return key.Verify(environment);
        }

        #endregion

        #region Custom encrypt providers

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "expirationDate"> The expiration date of this key. </param>
        /// <param name = "data">
        /// Additional data that determine the capabilities of the program, for example, the number
        /// launches or restriction on the use of certain functions. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <typeparam name = "TAlg">
        /// Algorithm type <see cref = "SymmetricAlgorithm" />,
        /// used to encrypt data.
        /// </typeparam>
        /// <typeparam name = "THash">
        /// Algorithm type <see cref = "HashAlgorithm" />,
        /// used to get the checksum.
        /// </typeparam> 
        public static ActivationKey Create<TAlg, THash>(DateTime expirationDate, object data = null, params object[] environment)
            where TAlg : SymmetricAlgorithm
            where THash : HashAlgorithm
        {
            ActivationKey activationKey = new ActivationKey();
            using (SymmetricAlgorithm cryptoAlg = Activator.CreateInstance<TAlg>())
            {
                activationKey.Seed = cryptoAlg.IV;
                using (DeriveBytes deriveBytes = new PasswordDeriveBytes(Serialize(environment), activationKey.Seed))
                {
                    cryptoAlg.Key = deriveBytes.GetBytes(cryptoAlg.KeySize / 8);
                }
                expirationDate = expirationDate.Date;
                long expirationDateStamp = expirationDate.ToBinary();
                using (ICryptoTransform transform = cryptoAlg.CreateEncryptor())
                {
                    byte[] encryptData = Serialize(expirationDateStamp, data);
                    activationKey.Data = transform.TransformFinalBlock(encryptData, 0, encryptData.Length);
                }
                using (HashAlgorithm hashAlg = Activator.CreateInstance<THash>())
                {
                    byte[] hashData = Serialize(expirationDateStamp, data, environment, activationKey.Seed);
                    activationKey.Hash = hashAlg.ComputeHash(hashData);
                }
            }
            return activationKey;
        }

        /// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "data">
        /// Additional data that determine the capabilities of the program, for example, the number
        /// launches or restriction on the use of certain functions. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <typeparam name = "TAlg">
        /// Algorithm type <see cref = "SymmetricAlgorithm" />,
        /// used to encrypt data.
        /// </typeparam>
        /// <typeparam name = "THash">
        /// Algorithm type <see cref = "HashAlgorithm" />,
        /// used to get the checksum.
        /// </typeparam> 
        public static ActivationKey Create<TAlg, THash>(object data = null, params object[] environment)
        where TAlg : SymmetricAlgorithm
        where THash : HashAlgorithm
        {
            return Create<TAlg, THash>(DateTime.MaxValue, data, environment);
        }

        /// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters and returns the decrypted application data.
        /// </summary>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <typeparam name = "TAlg">
        /// Algorithm type <see cref = "SymmetricAlgorithm" />,
        /// used to encrypt data.
        /// </typeparam>
        /// <typeparam name = "THash">
        /// Algorithm type <see cref = "HashAlgorithm" />,
        /// used to get the checksum.
        /// </typeparam>
        /// <returns> Decrypted data <see langword = "byte []" />,
        /// or <see langword = "null" /> if the activation key is invalid. </returns> 
        public byte[] GetData<TAlg, THash>(params object[] environment)
            where TAlg : SymmetricAlgorithm
            where THash : HashAlgorithm
        {
            if (InvalidState) return null;

            try
            {
                using (SymmetricAlgorithm cryptoAlg = Activator.CreateInstance<TAlg>())
                {
                    cryptoAlg.IV = Seed;
                    using (DeriveBytes deriveBytes = new PasswordDeriveBytes(Serialize(environment), Seed))
                    {
                        cryptoAlg.Key = deriveBytes.GetBytes(cryptoAlg.KeySize / 8);
                    }
                    using (ICryptoTransform transform = cryptoAlg.CreateDecryptor())
                    {
                        byte[] decryptedData = transform.TransformFinalBlock(Data, 0, Data.Length);
                        int dataLength = decryptedData.Length - 8;
                        if (dataLength < 0)
                        {
                            return null;
                        }
                        long expirationDateStamp = BitConverter.ToInt64(decryptedData, 0);
                        DateTime expirationDate = DateTime.FromBinary(expirationDateStamp);
                        if (expirationDate < DateTime.Today)
                        {
                            return null;
                        }
                        byte[] outputData;
                        if (dataLength > 0)
                        {
                            outputData = new byte[dataLength];
                            Buffer.BlockCopy(decryptedData, 8, outputData, 0, dataLength);
                        }
                        else
                        {
                            outputData = new byte[0];
                        }
                        using (HashAlgorithm hashAlg = Activator.CreateInstance<THash>())
                        {
                            byte[] hash = hashAlg.ComputeHash(Serialize(expirationDateStamp, decryptedData, environment, Seed));
                            return ByteArrayEquals(Hash, hash) ? outputData : null;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if the activation key is valid,
        /// represented by by string <paramref name="activationKey"/>
        /// for the specified environment parameters and returns the decrypted application data.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param> 
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <typeparam name = "TAlg">
        /// Algorithm type <see cref = "SymmetricAlgorithm" />,
        /// used to encrypt data.
        /// </typeparam>
        /// <typeparam name = "THash">
        /// Algorithm type <see cref = "HashAlgorithm" />,
        /// used to get the checksum.
        /// </typeparam>
        /// <returns> Decrypted data <see langword = "byte []" />,
        /// or <see langword = "null" /> if the activation key is invalid. </returns> 
        /// <exception cref="ArgumentException">Argument <paramref name="activationKey"/> is null or empty.</exception>
        public static byte[] GetData<TAlg, THash>(string activationKey, params object[] environment)
            where TAlg : SymmetricAlgorithm
            where THash : HashAlgorithm
        {
            using (ActivationKey key = Create<TAlg, THash>(activationKey)) return key.GetData<TAlg, THash>(environment);
        }

        /// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <typeparam name = "TAlg">
        /// Algorithm type <see cref = "SymmetricAlgorithm" />,
        /// used to encrypt data.
        /// </typeparam>
        /// <typeparam name = "THash">
        /// Algorithm type <see cref = "HashAlgorithm" />,
        /// used to get the checksum.
        /// </typeparam>
        /// <returns> <see langword = "true" /> if the activation key is valid. </returns> 
        public bool Verify<TAlg, THash>(params object[] environment)
            where TAlg : SymmetricAlgorithm
            where THash : HashAlgorithm
        {
            try
            {
                return GetData<TAlg, THash>(environment) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the activation key is valid,
        /// represented by by string <paramref name="activationKey"/>
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param> 
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <typeparam name = "TAlg">
        /// Algorithm type <see cref = "SymmetricAlgorithm" />,
        /// used to encrypt data.
        /// </typeparam>
        /// <typeparam name = "THash">
        /// Algorithm type <see cref = "HashAlgorithm" />,
        /// used to get the checksum.
        /// </typeparam>
        /// <returns> <see langword = "true" /> if the activation key is valid. </returns> 
        /// <exception cref="ArgumentException">Argument <paramref name="activationKey"/> is null or empty.</exception>
        public static bool Verify<TAlg, THash>(string activationKey, params object[] environment)
            where TAlg : SymmetricAlgorithm
            where THash : HashAlgorithm

        {
            using (ActivationKey key = Create<TAlg, THash>(activationKey)) return key.Verify<TAlg, THash>(environment);
        }

        #endregion

        #region Embeded classes

        #region Cipher

        // Port of cryptography provider designed by Ron Rives (C)
        private sealed class _ARC4 : IDisposable
        {
            private static readonly byte[] _A =
            {
                0x09, 0x0D, 0x11, 0x15, 0x19, 0x1d, 0x21, 0x25,
                0x29, 0x2d, 0x31, 0x35, 0x39, 0x3d, 0x41, 0x45,
                0x49, 0x4d, 0x51, 0x55, 0x59, 0x5d, 0x61, 0x65,
                0x69, 0x6d, 0x71, 0x75, 0x79, 0x7d, 0x81, 0x85,
                0x89, 0x8d, 0x91, 0x95, 0x99, 0x9d, 0xa1, 0xa5,
                0xa9, 0xad, 0xb1, 0xb5, 0xb9, 0xbd, 0xc1, 0xc5,
                0xc9, 0xcd, 0xd1, 0xd5, 0xd9, 0xdd, 0xe1, 0xe5,
                0xe9, 0xed, 0xf1, 0xf5, 0xf9
            };

            private static readonly byte[] _C =
            {
                0x05, 0x07, 0x0B, 0xD, 0x11, 0x13, 0x17, 0x1d,
                0x1f, 0x25, 0x29, 0x2b, 0x2f, 0x35, 0x3b, 0x3d,
                0x43, 0x47, 0x49, 0x4f, 0x53, 0x59, 0x61, 0x65,
                0x67, 0x6b, 0x6d, 0x71, 0x7f, 0x83, 0x89, 0x8b,
                0x95, 0x97, 0x9d, 0xa3, 0xa7, 0xad, 0xb3, 0xb5,
                0xbf, 0xc1, 0xc5, 0xc7, 0xd3, 0xdf, 0xe3, 0xe5,
                0xe9, 0xef, 0xf1, 0xfb
            };

            private byte[] _sblock = new byte[256];
            private int x = 0;
            private int y = 0;

            // Swaps the elements of the bytes array with indexes i and j.
            private static void Swap(byte[] bytes, int i, int j)
            {
                byte t = bytes[i];
                bytes[i] = bytes[j];
                bytes[j] = t;
            }

            // Performs initialization of the bytes array using LCR.
            private static void Init(byte[] bytes, byte[] iv)
            {
                if (iv == null)
                {
                    return;
                }
                int r = iv[0];
                int x = iv[1];
                int a = _A[iv[2] % _A.Length];
                int c = _C[iv[3] % _C.Length];
                const int m = 256;
                for (int i = 0; i < m; i++)
                {
                    bytes[i] = (byte)(r ^ (x = (a * x + c) % m));
                }
            }

            // Returns a pseudorandom number and change the state.
            private byte NextByte()
            {
                x = (x + 1) % 256;
                y = (y + _sblock[x]) % 256;
                Swap(_sblock, x, y);
                return _sblock[(_sblock[x] + _sblock[y]) % 256];
            }

            public _ARC4()
            {
                for (int i = 0; i < 256; i++)
                {
                    _sblock[i] = (byte)((uint)i ^ 0xAAu);
                }
            }

            public _ARC4(byte[] key, byte[] iv)
                : this()
            {
                Init(_sblock, iv);
                int keyLength;
                if (key != null && (keyLength = key.Length) != 0)
                {
                    int j = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        j = (j + _sblock[i] + key[i % keyLength]) % 256;
                        Swap(_sblock, i, j);
                    }
                }
            }

            // XORs each element of the buffer array, shifting its value by the amount returned by the NextByte function.
            private void Cipher(byte[] buffer, int offset, int count)
            {
                if (count != 0)
                {
                    for (int i = offset; i < count; i++)
                    {
                        buffer[i] = (byte)(buffer[i] ^ NextByte());
                    }
                }
            }

            // Encrypts the buffer array using the Cipher operation.
            public byte[] Cipher(byte[] buffer)
            {
                int length = buffer.Length;
                byte[] result = new byte[length];
                Array.Copy(buffer, result, length);
                Cipher(result, 0, length);
                return result;
            }

            // Encrypts data received as an array of objects using the Cipher algorithm.
            public byte[] Cipher(params object[] objects)
            {
                return Cipher(Serialize(objects));
            }

            public void Dispose()
            {
                if (_sblock != null) Array.Clear(_sblock, 0, 256);
                x = -1;
                y = -1;
                GC.SuppressFinalize(this);
            }
        }

        #endregion

        #region Hash

        // Port of MurmurHash3 algorithm designed by Austin Appleby(C).
        private sealed class _SMHasher
        {
            private const uint SEED = 3735928559u;
            private readonly uint _seed = SEED;

            public _SMHasher(uint seed = SEED)
            {
                _seed = seed;
            }

            // Calculates a 32-bit hash from a stream.
            public uint GetUInt32(Stream stream)
            {
                if (stream == null || !stream.CanRead)
                {
                    return 0u;
                }
                uint res = _seed;
                uint totalLength = 0u;
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int bufferLength;
                    byte[] buffer;
                    while ((bufferLength = (buffer = reader.ReadBytes(4)).Length) > 0)
                    {
                        totalLength += (uint)bufferLength;
                        uint part = buffer[0];
                        if (bufferLength > 1)
                        {
                            part |= (uint)(buffer[1] << 8);
                        }
                        if (bufferLength > 2)
                        {
                            part |= (uint)(buffer[2] << 16);
                        }
                        if (bufferLength > 3)
                        {
                            part |= (uint)(buffer[3] << 24);
                        }
                        part *= 3432918353u;
                        part = (part << 15) | (part >> 17);
                        part *= 461845907;
                        res ^= part;
                        if (bufferLength > 3)
                        {
                            res = (res << 13) | (res >> 19);
                            res = res * 5 + 3864292196u;
                        }
                    }
                }
                res ^= totalLength;
                res ^= res >> 16;
                res *= 2246822507u;
                res ^= res >> 13;
                res *= 3266489909u;
                return res ^ (res >> 16);
            }

            // Calculates a 32-bit hash of the specified bytes.
            public uint GetUInt32(byte[] bytes)
            {
                using (MemoryStream stream = new MemoryStream(bytes, false))
                {
                    return GetUInt32(stream);
                }
            }

            // Calculates a 32-bit hash of the specified objects.
            public uint GetUInt32(params object[] objects)
            {
                return GetUInt32(Serialize(objects));
            }

            // Calculates a 32-bit hash of the specified objects and returns it as 4 bytes.
            public byte[] GetBytes(params object[] objects)
            {
                return BitConverter.GetBytes(GetUInt32(objects));
            }
        }

        #endregion

        #region Encoding

        // Fork of encoder designed by Denis Zinchenko (C)
        private sealed class _Base32 : IDisposable
        {
            private string _encodingTable = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456";
            private byte[] _decodingTable = new byte[128];

            // Initializes the decoding and encoding tables.
            private void InitTable()
            {
                for (int j = 0; j < _decodingTable.Length; j++)
                {
                    _decodingTable[j] = byte.MaxValue;
                }
                for (int i = 0; i < _encodingTable.Length; i++)
                {
                    _decodingTable[_encodingTable[i]] = (byte)i;
                }
            }

            public _Base32()
            {
                InitTable();
            }

            // converts a byte array to a string using base32 encoding.
            public string Encode(byte[] data)
            {
                int dataLength;
                if (data == null || (dataLength = data.Length) == 0)
                {
                    return string.Empty;
                }
                StringBuilder buffer = new StringBuilder((int)Math.Ceiling((double)dataLength * 8.0 / 5.0));
                for (int i = 0; i < dataLength; i += 5)
                {
                    int byteCount = Math.Min(5, dataLength - i);
                    ulong cache = 0uL;
                    for (int j = 0; j < byteCount; j++)
                    {
                        cache = (cache << 8) | data[i + j];
                    }
                    for (int bitCount = byteCount * 8; bitCount > 0; bitCount -= 5)
                    {
                        int index = ((bitCount >= 5)
                            ? ((int)(cache >> bitCount - 5) & 0x1F)
                            : ((int)((long)cache & (long)(31 >> 5 - bitCount)) << 5 - bitCount));
                        buffer.Append(_encodingTable[index]);
                    }
                }
                return buffer.ToString();
            }

            // Decodes the data from the buffer string and returns it as a byte array.
            public byte[] Decode(string buffer)
            {
                if (string.IsNullOrEmpty(buffer))
                {
                    return new byte[0];
                }
                using (MemoryStream result = new MemoryStream((int)Math.Ceiling((double)buffer.Length * 5.0 / 8.0)))
                {
                    int[] index = new int[8];
                    int i = 0;
                    while (i < buffer.Length)
                    {
                        i = CreateIndexByOctetAndMovePosition(ref buffer, i, ref index);
                        int shortByteCount = 0;
                        ulong cache = 0uL;
                        for (int j = 0; j < 8 && index[j] != -1; j++)
                        {
                            cache = (cache << 5) | (ulong)(int)(_decodingTable[index[j]] & 0x1Fu);
                            shortByteCount++;
                        }
                        for (int bitCount = shortByteCount * 5; bitCount >= 8; bitCount -= 8)
                        {
                            result.WriteByte((byte)((cache >> bitCount - 8) & 0xFF));
                        }
                    }
                    return result.ToArray();
                }
            }

            // Parses the string data using the index array and moves the current position to the next character.
            private int CreateIndexByOctetAndMovePosition(ref string data, int currentPosition, ref int[] index)
            {
                int i = 0;
                while (i < 8)
                {
                    if (currentPosition >= data.Length)
                    {
                        index[i++] = -1;
                        continue;
                    }
                    char checkedSymbol = data[currentPosition];
                    if (checkedSymbol >= _decodingTable.Length || _decodingTable[checkedSymbol] == byte.MaxValue)
                    {
                        currentPosition++;
                        continue;
                    }
                    index[i] = data[currentPosition];
                    i++;
                    currentPosition++;
                }
                return currentPosition;
            }

            public void Dispose()
            {
                if (_decodingTable != null) Array.Clear(_decodingTable, 0, _decodingTable.Length);
                _decodingTable = null;
                _encodingTable = null;
            }
        }

        #endregion

        #endregion

        #region Static tools

        // Converts objects to a byte array. You can improve it however you find it necessary for your own stuff.
        [SecurityCritical]
        private static unsafe byte[] Serialize(params object[] objects)
        {
            using (MemoryStream memory = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memory))
            {
                for (int j = 0; j < objects.Length; j++)
                {
                    object obj = objects[j];
                    if (obj == null) continue;
                    try
                    {
                        switch (obj)
                        {
                            case SecureString secureString:
                                if (secureString == null || secureString.Length == 0)
                                    continue;
                                Encoding encoding = new UTF8Encoding();
                                int maxLength = encoding.GetMaxByteCount(secureString.Length);
                                IntPtr destPtr = Marshal.AllocHGlobal(maxLength);
                                IntPtr sourcePtr = Marshal.SecureStringToBSTR(secureString);
                                try
                                {
                                    char* chars = (char*)sourcePtr.ToPointer();
                                    byte* bptr = (byte*)destPtr.ToPointer();
                                    int length = encoding.GetBytes(chars, secureString.Length, bptr, maxLength);
                                    byte[] destBytes = new byte[length];
                                    for (int i = 0; i < length; ++i)
                                    {
                                        destBytes[i] = *bptr;
                                        bptr++;
                                    }
                                    writer.Write(destBytes);
                                }
                                finally
                                {
                                    Marshal.FreeHGlobal(destPtr);
                                    Marshal.ZeroFreeBSTR(sourcePtr);
                                }
                                continue;
                            case string str when str.Length > 0:
                                writer.Write(str.ToCharArray());
                                continue;
                            case DateTime date:
                                writer.Write(date.Ticks);
                                continue;
                            case bool @bool:
                                writer.Write(@bool);
                                continue;
                            case byte @byte:
                                writer.Write(@byte);
                                continue;
                            case sbyte @sbyte:
                                writer.Write(@sbyte);
                                continue;
                            case short @short:
                                writer.Write(@short);
                                continue;
                            case ushort @ushort:
                                writer.Write(@ushort);
                                continue;
                            case int @int:
                                writer.Write(@int);
                                continue;
                            case uint @uint:
                                writer.Write(@uint);
                                continue;
                            case long @long:
                                writer.Write(@long);
                                continue;
                            case ulong @ulong:
                                writer.Write(@ulong);
                                continue;
                            case float @float:
                                writer.Write(@float);
                                continue;
                            case double @double:
                                writer.Write(@double);
                                continue;
                            case decimal @decimal:
                                writer.Write(@decimal);
                                continue;
                            case Guid guid:
                                writer.Write(guid.ToByteArray());
                                continue;
                            case byte[] buffer when buffer.Length > 0:
                                writer.Write(buffer);
                                continue;
                            case char[] chars when chars.Length > 0:
                                writer.Write(chars);
                                continue;
                            case Array array when array.Length > 0:
                                foreach (object element in array) writer.Write(Serialize(element));
                                continue;
                            case IConvertible conv:
                                writer.Write(conv.ToString(CultureInfo.InvariantCulture));
                                continue;
                            case IFormattable frm:
                                writer.Write(frm.ToString(null, CultureInfo.InvariantCulture));
                                continue;
                            case Stream stream when stream.CanWrite:
                                stream.CopyTo(stream);
                                continue;
                            case ValueType @struct:
                                int size = Marshal.SizeOf(@struct);
                                byte[] bytes = new byte[size];
                                IntPtr handle = Marshal.AllocHGlobal(size);
                                try
                                {
                                    Marshal.StructureToPtr(@struct, handle, false);
                                    Marshal.Copy(handle, bytes, 0, size);
                                    writer.Write(bytes);
                                }
                                finally
                                {
                                    Marshal.FreeHGlobal(handle);
                                }
                                continue;
                            default:
                                if (!obj.GetType().IsSerializable)
                                    throw new SerializationException(GetResourceString("Arg_SerializationException"));
                                IFormatter formatter = new BinaryFormatter();
                                formatter.Serialize(memory, obj);
                                continue;
                        }
                    }
                    catch (Exception e)
                    {
#if DEBUG               // This is where the debugger information will be helpful
                        if (Debug.Listeners.Count == 0) continue;
                        Debug.WriteLine(DateTime.Now);
                        Debug.WriteLine(GetResourceString("Arg_ParamName_Name", $"{nameof(objects)}[{j}]"));
                        Debug.WriteLine(obj, "Object");
                        Debug.WriteLine(e, "Exception");
#endif
                    }
                }
                writer.Flush();
                byte[] result = memory.ToArray();
                return result;
            }
        }

        // Gets mscorlib internal error message.
        private static string GetResourceString(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (_mscorlib == null)
            {
                var assembly = Assembly.GetAssembly(typeof(object));
                var nameAssembly = assembly.GetName().Name;
                var manager = new ResourceManager(nameAssembly, assembly);
                _mscorlib = manager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            }
            return _mscorlib.GetString(name);
        }

        // Gets parametrized error message.
        private static string GetResourceString(string name, params object[] args)
        {
            return string.Format(GetResourceString(name), args);
        }

        // Checks whether array is empty or null.
        private static bool IsNullOrEmpty(byte[] bytes)
        {
            return bytes == null || bytes.Length == 0;
        }

        // Compares two byte arrays.
        private static bool ByteArrayEquals(byte[] bytes1, byte[] bytes2)
        {
            if ((bytes1 == null) ^ (bytes2 == null))
                return false;

            if (bytes1 == bytes2)
                return true;

            if (bytes1.Length != bytes2.Length)
                return false;

            for (int i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                    return false;
            }

            return true;
        }

        #endregion

        #region Methods

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

        // Analyzes the activation key, dividing it into parts and decode them.
        private void InternalParse(string activationKey)
        {
            if (string.IsNullOrEmpty(activationKey))
                return;

            string[] items = activationKey.Split('-');
            if (items.Length >= 3)
            {
                using (_Base32 base32 = new _Base32())
                {
                    Data = base32.Decode(items[0]);
                    Hash = base32.Decode(items[1]);
                    Seed = base32.Decode(items[2]);
                }
            }
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            if (InvalidState) return string.Empty;

            using (_Base32 base32 = new _Base32())
            {
                return base32.Encode(Data) + "-" + base32.Encode(Hash) + "-" + base32.Encode(Seed);
            }
        }

        /// <summary>
        /// Converts the current instance of <see cref="ActivationKey"/> to a string in the specified format.
        /// Replaces the characters %D, %H and %T in the string specified in <paramref name="pattern"/> 
        /// with <see cref="Data"/>, <see cref="Hash"/> and <see cref= "Seed"/> properties 
        /// of the current instance of <see cref="ActivationKey"/>.
        /// </summary>
        /// <param name="pattern">A string containing formatting information.</param>
        /// <returns>The string value of the current instance of <see cref="ActivationKey"/> in the specified format.</returns>
        /// <exception cref="ArgumentException">Argument <paramref name="format"/> is null or empty.</exception>
        public string ToString(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException(GetResourceString("Arg_EmptyOrNullString"), nameof(pattern));

            if (InvalidState) return string.Empty;

            using (_Base32 base32 = new _Base32())
            {
                StringBuilder cache = new StringBuilder(pattern)
                    .Replace("%D", base32.Encode(Data))
                    .Replace("%H", base32.Encode(Hash))
                    .Replace("%S", base32.Encode(Seed));
                return cache.ToString();
            }
        }

        /// <inheritdoc cref="IFormattable.ToString(string, IFormatProvider)"/>
        /// <exception cref="ArgumentException">Argument <paramref name="format"/> is null or empty.</exception>
        /// <exception cref="FormatException">Format specifier is unsupported.</exception>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                return ToString();

            if (InvalidState) return string.Empty;

            using (_Base32 base32 = new _Base32())
            {
                switch (format.ToUpperInvariant())
                {
                    case "D":
                        return IsNullOrEmpty(Data) ? string.Empty : base32.Encode(Data);
                    case "H":
                        return IsNullOrEmpty(Hash) ? string.Empty : base32.Encode(Hash);
                    case "S":
                        return IsNullOrEmpty(Seed) ? string.Empty : base32.Encode(Seed);
                    default:
                        throw new FormatException(GetResourceString("Format_BadFormatSpecifier"));
                }
            }
        }

        /// <inheritdoc cref="ICloneable.Clone()"/>
        public object Clone()
        {
            return new ActivationKey(ToString());
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
            if (reader == null) throw new ArgumentNullException(null, GetResourceString("ArgumentNull_WithParamName", nameof(reader)));
            InternalParse(reader.ReadContentAsString());
        }

        /// <inheritdoc cref="IXmlSerializable.WriteXml(XmlWriter)"/>
        /// <exception cref="ArgumentNullException">Argument <paramref name="writer"/> is null or empty.</exception>
        public void WriteXml(XmlWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(null, GetResourceString("ArgumentNull_WithParamName", nameof(writer)));
            writer.WriteString(ToString());
        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            if (Data != null) Array.Clear(Data, 0, Data.Length);
            if (Hash != null) Array.Clear(Hash, 0, Hash.Length);
            if (Seed != null) Array.Clear(Seed, 0, Seed.Length);
            Data = null;
            Hash = null;
            Seed = null;
        }

        #endregion
    }

    #region Type converter

    /// <summary>
    /// Converts <see cref = "ActivationKey" /> between other types.
    /// </summary> 
    public sealed class ActivationKeyConverter : TypeConverter
    {
        /// <inheritdoc cref="TypeConverter.CanConvertFrom(Type)"/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc cref="TypeConverter.CanConvertTo(Type)"/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string)) return true;
            return base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc cref="TypeConverter.ConvertFrom(ITypeDescriptorContext, CultureInfo, object)"/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string text) return new ActivationKey(text);
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc cref="TypeConverter.ConvertTo(ITypeDescriptorContext, CultureInfo, object, Type)"/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is ActivationKey activationKey)
                return activationKey.ToString();

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion
}
