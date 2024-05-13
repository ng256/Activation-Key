/***************************************************************

•   File: ActivationKeyEncryptor.cs

•   Description.
    ActivationKeyEncryptor   can be   used    to   protect  data
    associated with the  activation  of software or services. It
    allows  you  to generate unique  activation keys that can be
    used to authenticate and bind to a user-defined software and
    hardware environment.

    The code can be used  to create a software activation system
    or other products that require key activation verification.

***************************************************************/

using System.Security.Cryptography;
using static System.InternalTools;

namespace System.Security.Activation
{
    /// <summary>
    /// Implements functionality for generation of activation keys.
    /// Data can be encrypted using various encryption and hashing algorithms.
    /// </summary>
    public sealed class ActivationKeyEncryptor : IDisposable
    {
        private ICryptoTransform _encryptor;
        private HashAlgorithm _hasher;
        private byte[] _seed;

        /// <summary>
        /// Returns an instance of the <see cref="ActivationKeyEncryptor"/> using the default encryption algorithm.
        /// </summary>
        public static ActivationKeyEncryptor DefaultEncryptor => new ActivationKeyEncryptor();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyEncryptor"/> using default cryptographic algorithm.
        /// </summary>
        /// <param name="environment">Environment binding parameters used to generate the key.
        /// These parameters can be used to create a unique activation key that can be used to authenticate a user or device.</param>
        public ActivationKeyEncryptor(params object[] environment)
        {
            byte[] key = Serialize(environment);
            byte[] seed = GenerateRandom(sizeof(uint));
            _encryptor = new ARC4CryptoTransform(key, seed);
            _hasher = new SipHashAlgorithm(seed);
            _seed = seed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyEncryptor"/> using specified cryptographic algorithm.
        /// </summary>
        /// <param name="symmetricAlgorithm">Symmetric encryption algorithm.</param>
        /// <param name="hashAlgorithm">Hash algorithm.</param>
        /// <param name="environment">Environment binding parameters used to generate the key.
        /// These parameters can be used to create a unique activation key that can be used to authenticate a user or device.</param>
        /// <exception cref="ArgumentNullException"> If one of the arguments
        /// (<paramref name="symmetricAlgorithm"/> or <paramref name="hashAlgorithm"/>) is null.
        /// </exception>
        public ActivationKeyEncryptor(SymmetricAlgorithm symmetricAlgorithm, HashAlgorithm hashAlgorithm, params object[] environment)
        {
            if (symmetricAlgorithm == null) 
                throw new ArgumentNullException(nameof(symmetricAlgorithm), GetResourceString("ArgumentNull_WithParamName", nameof(symmetricAlgorithm)));
            if (hashAlgorithm == null) 
                throw new ArgumentNullException(nameof(hashAlgorithm), GetResourceString("ArgumentNull_WithParamName", nameof(hashAlgorithm)));

            byte[] seed = symmetricAlgorithm.IV;
            _hasher = hashAlgorithm;
            _seed = seed;
            using (PasswordDeriveBytes deriveBytes = new PasswordDeriveBytes(Serialize(environment), seed))
            {
                _encryptor = symmetricAlgorithm.CreateEncryptor(deriveBytes.GetBytes(symmetricAlgorithm.KeySize / 8), seed);
            }
        }

        /// <summary>
        /// Generates an <see cref="ActivationKey"/> based on the transferred data without a time limit for use.
        /// </summary>
        /// <param name="data">Data that will be stored in the key and can be retrieved when the key is verified.</param>
        /// <returns>The <see cref="ActivationKey"/> instance received by the current <see cref="ActivationKeyEncryptor"/> object.</returns>
        public ActivationKey Generate(params object[] data)
        {
            if (_encryptor == null || _hasher == null)
                throw new ObjectDisposedException(nameof(ActivationKeyEncryptor),GetResourceString("ObjectDisposed_Generic"));

            return Generate(DateTime.MaxValue.Date, data);
        }

        /// <summary>
        /// Generates an <see cref="ActivationKey"/> based on the data and expiration date.
        /// </summary>
        /// <param name="expirationDate">The key expiration date.</param>
        /// <param name="data">Data that will be included in the activation key.</param>
        /// <returns>Created activation key.</returns>
        public ActivationKey Generate(DateTime expirationDate, params object[] data)
        {
            byte[] serializedData = Serialize(expirationDate, data);
            byte[] encryptedData = _encryptor.TransformFinalBlock(serializedData, 0, serializedData.Length);
            byte[] seed = _seed;
            byte[] hash = _hasher.ComputeHash(Serialize(serializedData, seed));
            return new ActivationKey(encryptedData, hash, seed);
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            _encryptor?.Dispose();
            _hasher?.Dispose();
            _seed.Clear();
            _encryptor = null;
            _hasher = null;
            _seed = null;
        }
    }
}