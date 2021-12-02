/*************************************************************************************************************
System.Security.Cryptography.ActivationKey
Represents the activation key used to protect the licensed application.
Contains methods for updating the key based on the specified hardware and software environment.

Key format: DATA-HASH-TAIL. For Example, KCATBZ14Y4UEA-VGDM2ZQ-ATSVYMI.

Data 	A part of the key encrypted with a password. Contains the key expiration date and application options.
Hash 	Checksum of the key expiration date, password, options and environment parameters.
Tail	Initialization vector that used to decode the data.
**************************************************************************************************************/

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
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
	public sealed class ActivationKey : IFormattable, ICloneable, IDisposable, IXmlSerializable
	{
	private const int IVSIZE = 4; // Default length of the initialization vector.

	private static RNGCryptoServiceProvider _rng;

	private static RNGCryptoServiceProvider InternalRng => _rng ?? (_rng = new RNGCryptoServiceProvider());

	/// <summary>
	/// Password-encrypted part of the activation key. Contains expiration date of key and stored options.
	/// </summary>
	public byte[] Data
	{
		get;
		private set;
	}

	/// <summary>
	/// Checksum of key expiration date, password, options and environment parameters.
	/// </summary>
	public byte[] Hash
	{
		get;
		private set;
	}

	/// <summary>
	/// Initialization vector.
	/// </summary>
	public byte[] Tail
	{
		get;
		private set;
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "expirationDate"> The expiration date of this key. </param>
        /// <param name = "password"> Password. </param>
        /// <param name = "options">
        ///  Additional options that determine the capabilities of the program, for example, the number
        ///  launches or restriction on the use of certain functions. </param>
        /// <param name = "environment">
        ///  Additional parameters that define the firmware binding,
        ///  such as workstation id, application name, etc.
        /// </param> 
	public ActivationKey(DateTime expirationDate, byte[] password, object options = null, params object[] environment)
	{
		if (password == null)
		{
			password = new byte[0];
		}
		byte[] iv = new byte[IVSIZE];
		InternalRng.GetBytes(iv);
		using (_ARC4 arc4 = new _ARC4(password, iv))
		{
			expirationDate = expirationDate.Date;
			long expirationDateStamp = expirationDate.ToBinary();
			Data = arc4.Cipher(expirationDateStamp, options);
			_SMHasher mmh3 = new _SMHasher();
			Hash = mmh3.GetBytes(expirationDateStamp, password, options, environment, iv);
			Tail = iv;
		}
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
        /// Initializes a new instance <see cref = "ActivationKey" />.
        /// </summary> 
	public ActivationKey()
	{
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "password"> Password. </param>
        /// <param name = "options">
        /// Additional options that determine the capabilities of the program, for example, the number
        /// launches or restriction on the use of certain functions. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param> 
	public ActivationKey(byte[] password, object options = null, params object[] environment)
		: this(DateTime.MaxValue, password, options, environment)
	{
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "expirationDate"> The expiration date of this key. </param>
        /// <param name = "password"> Password. </param>
        /// <param name = "options">
        /// Additional options that determine the capabilities of the program, for example, the number
        /// launches or restriction on the use of certain functions. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param> 
		public ActivationKey(DateTime expirationDate, object password, object options = null, params object[] environment)
			: this(expirationDate, Serialize(password), options, environment)
		{
		}
		
		/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "password"> Password. </param>
        /// <param name = "options">
        /// Additional options that determine the capabilities of the program, for example, the number
        /// launches or restriction on the use of certain functions. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param> 
	public ActivationKey(object password, object options = null, params object[] environment)
		: this(DateTime.MaxValue, Serialize(password), options, environment)
	{
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using the constituent parts of the previously generated key.
        /// </summary>
        /// <param name = "data"> Encrypted part of the key. </param>
        /// <param name = "hash"> Key checksum. </param>
        /// <param name = "tail"> Key initialization vector. </param> 
	public ActivationKey(byte[] data, byte[] hash, byte[] tail)
	{
		Data = data ?? throw new ArgumentNullException("data");
		Hash = hash ?? throw new ArgumentNullException("hash");
		Tail = tail ?? throw new ArgumentNullException("tail");
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using textual representations of the constituent parts of the previously generated key.
        /// </summary>
        /// <param name = "data"> Encrypted part of the key. </param>
        /// <param name = "hash"> Key checksum. </param>
        /// <param name = "tail"> Key initialization vector. </param> 
	public ActivationKey(string data, string hash, string tail)
	{
		using (_Base32 base32 = new _Base32())
		{
			if (!string.IsNullOrEmpty(data))
			{
				Data = base32.Decode(data);
				if (!string.IsNullOrEmpty(hash))
				{
					Hash = base32.Decode(hash);
					if (!string.IsNullOrEmpty(tail))
					{
						Tail = base32.Decode(tail);
						return;
					}
					throw new ArgumentException(null, "tail");
				}
				throw new ArgumentException(null, "hash");
			}
			throw new ArgumentException(null, "data");
		}
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// using the text representation of the previously generated key.
        /// </summary>
        /// <param name = "activationKey"> A string containing the activation key. </param> 
	public ActivationKey(string activationKey)
	{
		InternalParse(activationKey);
	}
		
	/// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters and returns the decrypted application options.
        /// </summary>
        /// <param name = "password"> Password. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <returns> Get options <see langword = "byte []" />,
        /// or <see langword = "null" /> if the activation key is invalid. </returns> 
	public byte[] GetOptions(byte[] password = null, params object[] environment)
	{
		if (Data == null || Hash == null || Tail == null)
		{
			return null;
		}
		try
		{
			using (_ARC4 arc4 = new _ARC4(password, Tail))
			{
				byte[] data = arc4.Cipher(Data);
				int optionsLength = data.Length - 8;
				if (optionsLength < 0)
				{
					return null;
				}
				byte[] options;
				if (optionsLength > 0)
				{
					options = new byte[optionsLength];
					Buffer.BlockCopy(data, 8, options, 0, optionsLength);
				}
				else
				{
					options = new byte[0];
				}
				long expirationDateStamp = BitConverter.ToInt64(data, 0);
				DateTime expirationDate = DateTime.FromBinary(expirationDateStamp);
				if (expirationDate < DateTime.Today)
				{
					return null;
				}
				_SMHasher mmh3 = new _SMHasher();
				byte[] hash = mmh3.GetBytes(expirationDateStamp, password, options, environment, Tail);
				return ByteArrayEquals(Hash, hash) ? options : null;
			}
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "password"> Password. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <returns> Get options <see langword = "byte []" />,
        /// or <see langword = "null" /> if the activation key is invalid. </returns> 
	public byte[] GetOptions(object password, params object[] environment)
	{
		try
		{
			byte[] key = Serialize(password);
			return GetOptions(key, environment);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "password"> Password. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <returns> <see langword = "true" /> if the activation key is valid. </returns> 
	public bool Verify(byte[] password = null, params object[] environment)
	{
		try
		{
			return GetOptions(password, environment) != null;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "password"> Password. </param>
        /// <param name = "environment">
        /// Additional parameters that define the firmware binding,
        /// such as workstation id, application name, etc.
        /// </param>
        /// <returns> <see langword = "true" /> if the activation key is valid. </returns> 
	public bool Verify(object password = null, params object[] environment)
	{
		try
		{
			byte[] key = Serialize(password);
			return Verify(key, environment);
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "expirationDate"> The expiration date of this key. </param>
        /// <param name = "password"> Password. </param>
        /// <param name = "options">
        /// Additional options that determine the capabilities of the program, for example, the number
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
	public static ActivationKey Create<TAlg, THash>(DateTime expirationDate, byte[] password, object options = null, params object[] environment) where TAlg : SymmetricAlgorithm where THash : HashAlgorithm
	{
		ActivationKey activationKey = new ActivationKey();
		using (SymmetricAlgorithm cryptoAlg = Activator.CreateInstance<TAlg>())
		{
			if (password == null)
			{
				password = new byte[0];
			}
			activationKey.Tail = cryptoAlg.IV;
			using (DeriveBytes deriveBytes = new PasswordDeriveBytes(password, activationKey.Tail))
			{
				cryptoAlg.Key = deriveBytes.GetBytes(cryptoAlg.KeySize / 8);
			}
			expirationDate = expirationDate.Date;
			long expirationDateStamp = expirationDate.ToBinary();
			using (ICryptoTransform transform = cryptoAlg.CreateEncryptor())
			{
				byte[] data2 = Serialize(expirationDateStamp, options);
				activationKey.Data = transform.TransformFinalBlock(data2, 0, data2.Length);
			}
			using (HashAlgorithm hashAlg = Activator.CreateInstance<THash>())
			{
				byte[] data = Serialize(expirationDateStamp, cryptoAlg.Key, options, environment, activationKey.Tail);
				activationKey.Hash = hashAlg.ComputeHash(data);
			}
		}
		return activationKey;
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "password"> Password. </param>
        /// <param name = "options">
        /// Additional options that determine the capabilities of the program, for example, the number
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
	public static ActivationKey Create<TAlg, THash>(byte[] password, object options = null, params object[] environment) where TAlg : SymmetricAlgorithm where THash : HashAlgorithm
	{
		return Create<TAlg, THash>(DateTime.MaxValue, password, options, environment);
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "expirationDate"> The expiration date of this key. </param>
        /// <param name = "password"> Password. </param>
        /// <param name = "options">
        /// Additional options that determine the capabilities of the program, for example, the number
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
	public static ActivationKey Create<TAlg, THash>(DateTime expirationDate, object password, object options = null, params object[] environment) where TAlg : SymmetricAlgorithm where THash : HashAlgorithm
	{
		return Create<TAlg, THash>(expirationDate, Serialize(password), options, environment);
	}

	/// <summary>
        /// Initializes a new instance <see cref = "ActivationKey" />,
        /// actual for the specified environment parameters.
        /// </summary>
        /// <param name = "expirationDate"> The expiration date of this key. </param>
        /// <param name = "password"> Password. </param>
        /// <param name = "options">
        /// Additional options that determine the capabilities of the program, for example, the number
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
	public static ActivationKey Create<TAlg, THash>(object password, object options = null, params object[] environment) where TAlg : SymmetricAlgorithm where THash : HashAlgorithm
	{
		return Create<TAlg, THash>(DateTime.MaxValue, Serialize(password), options, environment);
	}

	/// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters and returns the decrypted application options ..
        /// </summary>
        /// <param name = "password"> Password. </param>
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
        /// <returns> Get options <see langword = "byte []" />,
        /// or <see langword = "null" /> if the activation key is invalid. </returns> 
	public byte[] GetOptions<TAlg, THash>(byte[] password = null, params object[] environment) where TAlg : SymmetricAlgorithm where THash : HashAlgorithm
	{
		if (Data == null || Hash == null || Tail == null)
		{
			return null;
		}
		try
		{
			using (SymmetricAlgorithm cryptoAlg = Activator.CreateInstance<TAlg>())
			{
				cryptoAlg.IV = Tail;
				using (DeriveBytes deriveBytes = new PasswordDeriveBytes(password, Tail))
				{
					cryptoAlg.Key = deriveBytes.GetBytes(cryptoAlg.KeySize / 8);
				}
				using (ICryptoTransform transform = cryptoAlg.CreateDecryptor())
				{
					byte[] data = transform.TransformFinalBlock(Data, 0, Data.Length);
					int optionsLength = data.Length - 8;
					if (optionsLength < 0)
					{
						return null;
					}
					byte[] options;
					if (optionsLength > 0)
					{
						options = new byte[optionsLength];
						Buffer.BlockCopy(data, 8, options, 0, optionsLength);
					}
					else
					{
						options = new byte[0];
					}
					long expirationDateStamp = BitConverter.ToInt64(data, 0);
					DateTime expirationDate = DateTime.FromBinary(expirationDateStamp);
					if (expirationDate < DateTime.Today)
					{
						return null;
					}
					using (HashAlgorithm hashAlg = Activator.CreateInstance<THash>())
					{
						byte[] hash = hashAlg.ComputeHash(Serialize(expirationDateStamp, cryptoAlg.Key, options, environment, Tail));
						return ByteArrayEquals(Hash, hash) ? options : null;
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
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "password"> Password. </param>
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
        /// <returns> Get options <see langword = "byte []" />,
        /// or <see langword = "null" /> if the activation key is invalid. </returns> 
	public byte[] GetOptions<TAlg, THash>(object password, params object[] environment) where TAlg : SymmetricAlgorithm where THash : HashAlgorithm
	{
		try
		{
			byte[] passwordBytes = Serialize(password);
			return GetOptions<TAlg, THash>(passwordBytes, environment);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "password"> Password. </param>
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
	public bool Verify<TAlg, THash>(byte[] password = null, params object[] environment) where TAlg : SymmetricAlgorithm where THash : HashAlgorithm
	{
		try
		{
			return GetOptions<TAlg, THash>(password, environment) != null;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
        /// Checks if the activation key is valid,
        /// represented by the current instance <see cref = "ActivationKey" />
        /// for the specified environment parameters.
        /// </summary>
        /// <param name = "password"> Password. </param>
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
	public bool Verify<TAlg, THash>(object password = null, params object[] environment) where TAlg : SymmetricAlgorithm where THash : HashAlgorithm
	{
		try
		{
			byte[] key = Serialize(password);
			return Verify<TAlg, THash>(key, environment);
		}
		catch
		{
			return false;
		}
	}
		

	private sealed class _ARC4 : IDisposable
	{
		private byte[] _sblock = new byte[256];

		private int x = 0;

		private int y = 0;

		private bool _disposed = false;

		private static void Swap(byte[] bytes, int i, int j)
		{
			byte t = bytes[i];
			bytes[i] = bytes[j];
			bytes[j] = t;
		}

		private static void Init(byte[] bytes, byte[] iv)
		{
			if (iv == null)
			{
				return;
			}
			for (int j = 0; j < iv.Length; j++)
			{
				byte b = iv[j];
				for (int i = 0; i < 256; i++)
				{
					b = (byte)(b ^ (byte)(~i ^ j));
					Swap(bytes, i, b);
				}
			}
		}

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

		public byte[] Cipher(byte[] buffer)
		{
			int length = buffer.Length;
			byte[] result = new byte[length];
			Array.Copy(buffer, result, length);
			Cipher(result, 0, length);
			return result;
		}

		public byte[] Cipher(params object[] objects)
		{
			return Cipher(Serialize(objects));
		}

		private void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				try
				{
					if (_sblock != null)
					{
						Array.Clear(_sblock, 0, 256);
					}
				}
				finally
				{
					_disposed = true;
				}
			}
			_sblock = null;
			x = -1;
			y = -1;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~_ARC4()
		{
			Dispose(false);
		}
	}

	private sealed class _SMHasher // Port of Austin Appleby's MurmurHash3 algorithm.
	{
		private const uint SEED = 3735928559u;

		public uint Seed
		{
			get;
		} = 3735928559u;


		public _SMHasher(uint seed = 3735928559u)
		{
			Seed = seed;
		}

		public uint GetUInt32(Stream stream)
		{
			if (stream == null || !stream.CanRead)
			{
				return 0u;
			}
			uint res = Seed;
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

		public uint GetUInt32(byte[] bytes)
		{
			using (MemoryStream stream = new MemoryStream(bytes, false))
			{
				return GetUInt32(stream);
			}
		}

		public uint GetUInt32(params object[] objects)
		{
			return GetUInt32(Serialize(objects));
		}

		public byte[] GetBytes(params object[] objects)
		{
			return BitConverter.GetBytes(GetUInt32(objects));
		}
	}

	private sealed class _Base32 : IDisposable
	{
		private const string _encodingTable = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456";
		private readonly byte[] _decodingTable = new byte[128];

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
					int index = ((bitCount >= 5) ? ((int)(cache >> bitCount - 5) & 0x1F) : ((int)((long)cache & (long)(31 >> 5 - bitCount)) << 5 - bitCount));
					buffer.Append(_encodingTable[index]);
				}
			}
			return buffer.ToString();
		}

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
			Array.Clear(_decodingTable, 0, _decodingTable.Length);
			_decodingTable = null;
		}
	}

	[SecurityCritical]
	static unsafe byte[] Serialize(params object[] objects)
	{
		using (MemoryStream memory = new MemoryStream())
		using (BinaryWriter writer = new BinaryWriter(memory))
		{
			foreach (object obj in objects)
			{
				if (obj == null) continue;
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
					case string str:
						if (str.Length > 0)
							writer.Write(str.ToCharArray());
						continue;
					case DateTime date:
						writer.Write(date.Ticks);
						continue;
					case bool @bool:
						writer.Write(@bool);
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
					case byte[] buffer:
						if (buffer.Length > 0)
							writer.Write(buffer);
						continue;
					case Array array:
						if (array.Length > 0)
							foreach (var a in array) writer.Write(Serialize(a));
						continue;
					case IConvertible conv:
						writer.Write(conv.ToString(CultureInfo.InvariantCulture));
						continue;
					case IFormattable frm:
						writer.Write(frm.ToString(null, CultureInfo.InvariantCulture));
						continue;
					case Stream stream:
						stream.CopyTo(stream);
						continue;
					default:
						try
						{
							int rawsize = Marshal.SizeOf(obj);
							byte[] rawdata = new byte[rawsize];
							GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
							Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
							writer.Write(rawdata);
							handle.Free();
						}
						catch(Exception e)
						{

						}
						continue;
				}
			}
			writer.Flush();
			byte[] bytes = memory.ToArray();
			return bytes;
		}
	}

	private static bool ByteArrayEquals(byte[] b1, byte[] b2)
	{
		if ((b1 == null) ^ (b2 == null))
		{
			return false;
		}
		if (b1 == b2)
		{
			return true;
		}
		if (b1.Length != b2.Length)
		{
			return false;
		}
		for (int i = 0; i < b1.Length; i++)
		{
			if (b1[i] != b2[i])
			{
				return false;
			}
		}
		return true;
	}

	private void InternalParse(string activationKey)
	{
		if (string.IsNullOrEmpty(activationKey))
		{
			return;
		}
		string[] items = activationKey.Split('-');
		if (items.Length >= 3)
		{
			using (_Base32 base32 = new _Base32())
			{
				Data = base32.Decode(items[0]);
				Hash = base32.Decode(items[1]);
				Tail = base32.Decode(items[2]);
			}
		}
	}

	public static explicit operator string(ActivationKey activationKey)
	{
		return activationKey.ToString();
	}

	public static explicit operator ActivationKey(string activationKey)
	{
		return new ActivationKey(activationKey);
	}

	/// <inheritdoc cref="object.ToString()"/>
	public override string ToString()
	{
		if (Data == null || Hash == null || Tail == null)
		{
			return string.Empty;
		}
		using (_Base32 base32 = new _Base32())
		{
			return base32.Encode(Data) + "-" + base32.Encode(Hash) + "-" + base32.Encode(Tail);
		}
	}
		
	/// <inheritdoc cref="ICloneable.Clone()"/>
	public object Clone()
	{
		return new ActivationKey(ToString());
	}
		
	/// <inheritdoc cref="IFormattable.ToString(string, IFormatProvider)"/>
	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (Data == null || Hash == null || Tail == null) return string.Empty;
		    using (_Base32 base32 = new _Base32())
		    {
			return format
			    .Replace("{data}", base32.Encode(Data))
			    .Replace("{hash}", base32.Encode(Hash))
			    .Replace("{tail}", base32.Encode(Tail));
		    }
		}
		
	/// <inheritdoc cref="IDisposable.Dispose()"/>
	public void Dispose()
	{
	    if (Data != null) Array.Clear(Data, 0, Data.Length);
	    if (Hash != null) Array.Clear(Hash, 0, Hash.Length);
	    if (Tail != null) Array.Clear(Tail, 0, Tail.Length);
	    Data = null;
	    Hash = null;
	    Tail = null;
	}

	/// <inheritdoc cref="IXmlSerializable.GetSchema()"/>
	public XmlSchema GetSchema()
	{
		return null;
	}

	/// <inheritdoc cref="IXmlSerializable.ReadXml(XmlReader)"/>
	public void ReadXml(XmlReader reader)
	{
		InternalParse(reader.ReadContentAsString());
	}

	/// <inheritdoc cref="IXmlSerializable.WriteXml(XmlWriter)"/>
	public void WriteXml(XmlWriter writer)
	{
		writer.WriteString(ToString());
	}
    }

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
}
