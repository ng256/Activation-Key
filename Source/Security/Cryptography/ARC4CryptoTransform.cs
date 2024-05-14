/***************************************************************

•   File: ARC4CryptoTransform.cs

•   Description.

    ARC4CryptoTransform  implements and    provides methods  for
    encrypting   and  decrypting    data using  a   modified RC4
    algorithm. Used by default    to  generate  activation keys.

    Despite  the known vulnerabilities  of RC4, such  as leaking
    key information and the possibility of attacking  weak keys,
    it is sufficient for the purposes of this project, since the
    length of the encrypted  data,  as a rule, does not exceed a
    few    bytes. To    improve    cryptographic   strength,  an
    initialization vector and  skipping the first 512 bytes were
    implemented.

***************************************************************/

using static System.InternalTools;

namespace System.Security.Cryptography
{
    internal sealed class ARC4CryptoTransform : ICryptoTransform
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
        private int _x;
        private int _y;
        private byte[] _key;
        private byte[] _iv;
        private bool _disposed;

        // Size of the input data block in bits.
        public int InputBlockSize => 8;

        // Size of the output data block in bits.
        public int OutputBlockSize => 8;

        // Indicates whether multiple data blocks can be converted.
        public bool CanTransformMultipleBlocks => true;

        // Indicates whether the transformation can be reused.
        public bool CanReuseTransform => true;

        public ARC4CryptoTransform()
        {
            Initialize();
        }

        public ARC4CryptoTransform(uint seed)
        {
            _iv = GetBytes(seed);
            _key = GetBytes(seed);
            Reset();
        }

        public ARC4CryptoTransform(byte[] key)
        {
            _key = key.ArrayClone();
            Reset();
        }

        public ARC4CryptoTransform(byte[] key, uint seed)
        {
            _iv = GetBytes(seed);
            _key = key.ArrayClone();
            Reset();
        }

        public ARC4CryptoTransform(byte[] key, byte[] iv)
        {
            _iv = iv.ArrayClone();
            _key = key.ArrayClone();
            Reset();
        }

        private static void Swap(byte[] array, int x, int y)
        {
            byte num = array[x];
            array[x] = array[y];
            array[y] = num;
        }

        // Processing pseudo-random generation algorithm.
        private byte NextByte()
        {
            _x = (_x + 1) % 256;
            _y = (_y + _sblock[_x]) % 256;
            Swap(_sblock, _x, _y);
            return _sblock[(_sblock[_x] + _sblock[_y]) % 256];
        }

        private void Reset()
        {
            _x = 0;
            _y = 0;
            if (_iv.IsNullOrEmpty())
                Initialize();
            else
                InitializeUsingLCR(_iv);
            if (!_key.IsNullOrEmpty())
                InitializeUsingKSA(_key);
            
            DropDown(512); // Just skips 512 bytes.
        }

        // Initializes the sblock with default values.
        private void Initialize()
        {
            for (int i = 0; i < 256; i++)
            {
                _sblock[i] = (byte)i;
            }
        }

        // Initializes the sblock using key-scheduling algorithm.
        private void InitializeUsingKSA(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), GetResourceString("ArgumentNull_Key"));

            for (int i = 0, j = 0, l = key.Length; i < 256; i++)
            {
                j = (j + _sblock[i] + key[i % l]) % 256;
                Swap(_sblock, i, j);
            }
        }

        // Initializes the sblock using linear congruential random.
        private void InitializeUsingLCR(byte[] iv)
        {
            if (iv == null)
                throw new ArgumentNullException(nameof(iv), GetResourceString("ArgumentNull_Array"));
            if (iv.Length < 4)
                throw new ArgumentException(GetResourceString("Cryptography_InvalidIVSize"), nameof(iv));

            int r = iv[0];
            int x = iv[1];
            int a = _A[iv[2] % _A.Length];
            int c = _C[iv[3] % _C.Length];
            const int m = 256;
            for (int i = 0; i < m; i++)
            {
                _sblock[i] = (byte) (r ^ (x = (a * x + c) % m));
            }

            DropDown(r + a + c + m); // Skips from 256 to 1024 bytes.
        }

        // Skips a specified number of first random bytes to be discarded.
        public void DropDown(int count)
        {
            for (int i = 0; i < count; i++)
            {
                NextByte();
            }
        }

        // Converts a block of data using the RC4 algorithm.
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset)
        {
            CheckDisposed();
            CheckBufer(inputBuffer, inputOffset, inputCount);
            CheckBufer(outputBuffer, outputOffset);

            int startOutputOffset = outputOffset;
            for (int i = inputOffset; i < inputOffset + inputCount; i++)
            {
                outputBuffer[outputOffset++] = unchecked((byte)(inputBuffer[i] ^ NextByte()));
            }

            return outputOffset - startOutputOffset;
        }

        // Converts the last block of data using the RC4 algorithm.
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            CheckDisposed();
            CheckBufer(inputBuffer, inputOffset, inputCount);

            byte[] outputBuffer = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            Reset();

            return outputBuffer;
        }

        private void CheckBufer(byte[] buffer, int offset)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), GetResourceString("ArgumentNull_Buffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset,
                    GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), offset,
                    GetResourceString("Argument_InvalidValue"));
        }

        private void CheckBufer(byte[] buffer, int offset, int count)
        {
            CheckBufer(buffer, offset);
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), offset,
                    GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count), count, GetResourceString("Argument_InvalidValue"));
            if (buffer.Length - count < offset)
                throw new ArgumentException(GetResourceString("Argument_InvalidOffLen"));
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ARC4CryptoTransform),GetResourceString("ObjectDisposed_Generic"));
        }

        // Releases resources used by the class.
        public void Dispose()
        {
            _sblock.Clear();
            _key.Clear();
            _iv.Clear();

            _sblock = null;
            _key = null;
            _iv = null;

            _x = 0;
            _y = 0;

            _disposed = true;
        }
    }
}
