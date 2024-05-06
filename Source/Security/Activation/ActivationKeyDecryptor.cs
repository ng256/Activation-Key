/***************************************************************

•   File: ActivationKeyDecryptor.cs

•   Description.

	ActivationKeyDecryptor is  a class that is used to check the
	activation of a key and  decrypt  the data contained  in it,
	using various encryption and hashing  algorithms, as well as
	binding  to user-defined hardware  and  software environment
	variables.

	The code can be used  to create a software activation system
	or other products that require key activation verification.

***************************************************************/

using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static System.InternalTools;

namespace System.Security.Activation
{
    /// <summary>
    /// Used to verify the activation key and decrypt the data contained in it.
    /// Data can be encrypted using various encryption and hashing algorithms.
    /// </summary>
    public sealed class ActivationKeyDecryptor : IDisposable
    {
        private byte[] _data = null;

        /// <summary>
        /// Returns a byte array containing the decrypted data
        /// </summary>
        public byte[] Data => _data?.ArrayClone();

        /// <summary>
        /// Returns true if the data was successfully decrypted. False means the key failed verification.
        /// </summary>
        public bool Success => _data != null;


        /// <summary>
        /// Returns the activation key expiration date.
        /// </summary>
        public DateTime ExpirationDate { get; private set; } = DateTime.MinValue.Date;

        // Decrypts the data contained in the activation key using the specified encryption and hashing algorithm.
        private unsafe void  InternalDecrypt(ICryptoTransform transform, HashAlgorithm hashAlgorithm,
            ActivationKey activationKey)
        {
            if (activationKey.InvalidState)
                return;

            byte[] data = activationKey.Data;
            byte[] decryptedData = transform.TransformFinalBlock(data, 0, data.Length);
            byte[] seed = activationKey.Seed;
            byte[] hash = hashAlgorithm.ComputeHash(Serialize(decryptedData, seed));

            if (decryptedData.Length < sizeof(DateTime)) // The data block is too short for DateTime.
                return;

            ExpirationDate = ConvertTo<DateTime>(decryptedData);
            if (ExpirationDate.Date >= DateTime.Today.Date && hash.ArrayEquals(activationKey.Hash))
                _data = decryptedData.ArrayClone(sizeof(DateTime));
        }

        // The invisible constructor.
        internal ActivationKeyDecryptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyDecryptor"/>,
        /// which is used to decrypt the data contained in the <see cref="ActivationKey"/>.
        /// </summary>
        /// <param name="activationKey">The activation key to be verified.</param>
        /// <param name="environment">Environment binding parameters used to generate the key.
        /// These parameters can be used to create a unique activation key that can be used to authenticate a user or device.</param>
        /// <exception cref="ArgumentNullException">Trows if argument <paramref name="activationKey"/> is null.</exception>
        public ActivationKeyDecryptor(ActivationKey activationKey, params object[] environment)
        {
            if (activationKey == null)
                throw new ArgumentNullException(nameof(activationKey), 
                    GetResourceString("ArgumentNull_WithParamName", nameof(activationKey)));

            try
            {
                byte[] seed = activationKey.Seed;
                using (ICryptoTransform decryptor = new ARC4CryptoTransform(Serialize(environment), seed))
                using (HashAlgorithm hasher = new SipHashAlgorithm(seed))
                {
                    InternalDecrypt(decryptor, hasher, activationKey);
                }
            }
            catch (Exception e)
            {
                _data = null;
#if DEBUG               
                if (Debug.Listeners.Count > 0)
                {
                    Debug.WriteLine(DateTime.Now);
                    Debug.WriteLine(e, "Exception");
                }
#endif
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyDecryptor"/>, 
        /// </summary>
        /// <param name="symmetricAlgorithm">Symmetric encryption algorithm.</param>
        /// <param name="hashAlgorithm">Hash algorithm.</param>
        /// <param name="activationKey">The activation key to be verified.</param>
        /// <param name="environment">Environment binding parameters used to generate the key.
        /// These parameters can be used to create a unique activation key that can be used to authenticate a user or device.</param>
        /// <exception cref="ArgumentNullException"> If one of the arguments (<paramref name="activationKey"/>,
        /// <paramref name="symmetricAlgorithm"/> or <paramref name="hashAlgorithm"/>) is null.
        /// </exception>
        public ActivationKeyDecryptor(SymmetricAlgorithm symmetricAlgorithm, HashAlgorithm hashAlgorithm, 
            ActivationKey activationKey, params object[] environment)
        {
            if (symmetricAlgorithm == null)
                throw new ArgumentNullException(nameof(symmetricAlgorithm), 
                    GetResourceString("ArgumentNull_WithParamName", nameof(symmetricAlgorithm)));
            if (hashAlgorithm == null)
                throw new ArgumentNullException(nameof(hashAlgorithm), 
                    GetResourceString("ArgumentNull_WithParamName", nameof(hashAlgorithm)));
            if (activationKey == null)
                throw new ArgumentNullException(nameof(activationKey), 
                    GetResourceString("ArgumentNull_WithParamName", nameof(activationKey)));

            try
            {
                byte[] seed = activationKey.Seed;
                using(PasswordDeriveBytes deriveBytes = new PasswordDeriveBytes(Serialize(environment), seed))
                using(ICryptoTransform decryptor = symmetricAlgorithm.CreateDecryptor(deriveBytes.GetBytes(symmetricAlgorithm.KeySize / 8), seed))
                {
                    InternalDecrypt(decryptor, hashAlgorithm, activationKey);
                }
            }
            catch (Exception e)
            {
                _data = null;
#if DEBUG               
                if (Debug.Listeners.Count > 0)
                {
                    Debug.WriteLine(DateTime.Now);
                    Debug.WriteLine(e, "Exception");
                }
#endif
            }
        }

        /// <summary>
        /// Returns a <see cref="BinaryReader"/> object that can be used to read the decrypted data stored to the activation key.
        /// </summary>
        /// <returns>A <see cref="BinaryReader"/> object for reading the decrypted data.</returns>
        /// <exception cref="InvalidOperationException">
        /// Key verification failed, the key did not contain encrypted data, or the key has expired.
        /// </exception>
        public BinaryReader GetBinaryReader()
        {
            if (_data.IsNullOrEmpty())
                throw new InvalidOperationException(GetResourceString("Arg_InvalidOperationException"));

            return new BinaryReader(new MemoryStream(Data, false));
        }

        /// <summary>
        /// Returns a <see cref="TextReader"/> object that can be used to read the decrypted data stored to the activation key.
        /// </summary>
        /// <param name="encoding">The encoding applied to the contents of the decrypted data.</param>
        /// <returns>A <see cref="TextReader"/> object for reading the decrypted data.</returns>
        /// <exception cref="InvalidOperationException">
        /// Key verification failed, the key did not contain encrypted data, or the key has expired.
        /// </exception>
        public TextReader GetTextReader(Encoding encoding = null)
        {
            if (_data.IsNullOrEmpty())
                throw new InvalidOperationException(GetResourceString("Arg_InvalidOperationException"));

            return new StreamReader(new MemoryStream(Data, false), encoding ?? Encoding.UTF8);
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            _data.Clear();
            _data = null;
        }
    }
}