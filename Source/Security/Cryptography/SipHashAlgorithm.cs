/****************************************************************

•   File: SipHashAlgorithm.cs

•   Description.

    SipHashAlgorithm provides the ability to create cryptographic  
    hash  functions  based on  a modified SipHash algorithm.

    The default  hash algorithm  choice  is SipHash, which works
    very well for short input data. In this  implementation, the
    length of the resulting hash is 32 bits, which is sufficient
    for the purposes of this project.


****************************************************************/

using static System.InternalTools;

namespace System.Security.Cryptography
{
    // Provides the ability to create cryptographic hash functions based on the SipHash algorithm.
    // This allows the class to be used to check the integrity of the activation key data.
    internal sealed class SipHashAlgorithm : HashAlgorithm
    {
        // Returns the default hash generator.
        public static  SipHashAlgorithm DefaultHash => new SipHashAlgorithm();

        // An array of four uint elements that are used to store intermediate values when calculating the hash function.
        private uint[] _buffer = { 0x736F6D65, 0x646F7261, 0x1F160A00, 0x100A1603 };

        // The constructor initializes the elements of the _buffer array using the seed parameter.
        private uint _seed;

        // Sets the hash size to the size of an integer.
        private SipHashAlgorithm()
        {
            HashSizeValue = 32;
        }

        // Accepts a seed parameter. The constructor initializes the elements of the _buffer array using the seed parameter.
        // Seed is parameter that is used to initialize the initial state of the algorithm
        public SipHashAlgorithm(uint seed = 0) : this()
        {
            _seed = seed;
            Initialize();
        }

        // Takes a bytes array and uses it to initialize the elements of the _buffer array.
        public unsafe SipHashAlgorithm(byte[] iv) : this()
        {
            if (iv == null)
                throw new ArgumentNullException(nameof(iv), GetResourceString("ArgumentNull_Key"));
            if (iv.Length < 4)
                throw new ArgumentException(GetResourceString("Cryptography_InvalidIVSize"), nameof(iv));

            _seed = ConvertTo<uint>(iv);
            Initialize();
        }

        // Initializes the elements of the _buffer array.
        public override void Initialize()
        {
            _buffer[0] = 0x736F6D65 ^ _seed;
            _buffer[1] = 0x646F7261 ^ _seed;
            _buffer[2] = 0x1F160A00 ^ _buffer[0];
            _buffer[3] = 0x100A1603 ^ _buffer[1];
        }

        // Returns a hash value obtained from a byte array using the SipHash algorithm.
        public int GetInt32(byte[] bytes)
        {
            return ConvertTo<int>(ComputeHash(bytes), 0);
        }

        // Returns a hash value obtained from a byte array using the SipHash algorithm.
        private uint GetUInt32(byte[] bytes)
        {
            return ConvertTo<uint>(ComputeHash(bytes), 0);
        }

        // Rearranges the elements of the _buffer array. The method is used in the SipRound method to shuffle values.
        private void Rotate()
        {
            _buffer[0] += _buffer[1] ^ _buffer[2] ^ _buffer[3];
            uint swap = _buffer[0];
            _buffer[0] = _buffer[1];
            _buffer[1] = _buffer[2];
            _buffer[2] = _buffer[3];
            _buffer[3] = swap;
        }

        // Performs one round of calculations in the SipHash algorithm.
        private void SipRound()
        {
            _buffer[0] += _buffer[1];
            _buffer[1] <<= 13;
            _buffer[1] ^= _buffer[0];
            _buffer[0] <<= 32;

            _buffer[2] += _buffer[3];
            _buffer[3] <<= 16;
            _buffer[3] ^= _buffer[2];

            _buffer[0] += _buffer[3];
            _buffer[3] <<= 21;
            _buffer[3] ^= _buffer[0];

            _buffer[2] += _buffer[1];
            _buffer[1] <<= 17;
            _buffer[1] ^= _buffer[2];
            _buffer[2] <<= 32;

            Rotate();
            Rotate();
            Rotate();
        }

        // Calculates a hash value from a byte array using the SipHash algorithm.
        protected override unsafe void HashCore(byte[] array, int ibStart, int cbSize)
        {
            uint lenght = (uint)cbSize;
            uint b = ((uint)lenght) << 24;

            if (lenght > 0)
            {
                fixed (byte* bytes = &array[ibStart])
                {
                    byte* @byte = bytes;
                    uint tailCount = lenght & 7;
                    byte* ptrSeed = @byte + lenght - tailCount;

                    while (@byte < ptrSeed)
                    {
                        _buffer[0] ^= (uint)@byte[0];
                        _buffer[1] ^= (uint)@byte[1] << 8;
                        _buffer[2] ^= (uint)@byte[2] << 16;
                        _buffer[3] ^= (uint)@byte[3] << 24;

                        SipRound();
                        SipRound();

                        @byte += 8;
                    }

                    for (int i = 0; i < tailCount; ++i)
                    {
                        _buffer[3] ^= b;
                        b |= (uint)ptrSeed[i] << (8 * i);
                        _buffer[0] ^= b;

                    }
                }
            }

            _buffer[3] ^= b;

            SipRound();
            SipRound();

            _buffer[0] ^= b;
            _buffer[2] ^= 0xff;

            SipRound();
            SipRound();
            SipRound();
            SipRound();
        }

        // Returns the final hash value.
        protected override unsafe byte[] HashFinal()
        {
            byte[] hashValue = new byte[4];
            fixed (byte* buffer = hashValue)
            {
                for (var i = 0; i < _buffer.Length; i++)
                {
                    *(uint*)buffer ^= _buffer[i];
                }
            }
            Initialize();
            return hashValue;
        }

        // Releases resources used by the class.
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _buffer.Clear();
            _buffer = null;
            _seed = 0;
            base.Dispose(true);
        }
    }
}
