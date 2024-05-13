/***************************************************************

•   File: ActivationKeyManager.cs

•   Description.

    ActivationKeyManager is  designed to   load and save an
    activation key from various sources.

    Provides the following features:

        - Loading an activation key from a file.
        - Loading the activation key from the INI file.
        - Downloading the activation key from the registry.
        - Saving the activation key to a file.  

***************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using static System.InternalTools;

namespace System.Security.Activation
{
    /// <summary>
    /// Provides methods for loading and saving an activation key using various sources.
    /// </summary>
    public class ActivationKeyManager
    {
        private readonly ActivationKeyTextParser _textParser = ActivationKeyTextParser.DefaultParser;
        private readonly ActivationKeyBinaryParser _binaryParser = ActivationKeyBinaryParser.DefaultParser;

        /// <summary>
        /// Returns the default instance of the <see cref="ActivationKeyManager"/>.
        /// </summary>
        public static ActivationKeyManager DefaultManager => new ActivationKeyManager();

        /// <summary>
        /// Returns a new instance of the <see cref="ActivationKeyManager"/> using decimal encoding.
        /// </summary>
        public static ActivationKeyManager Base10Manager => new ActivationKeyManager(PrintableEncoding.Decimal);

        /// <summary>
        /// Returns a new instance of the <see cref="ActivationKeyManager"/> using hexadecimal encoding.
        /// </summary>
        public static ActivationKeyManager Base16Manager => new ActivationKeyManager(PrintableEncoding.Hexadecimal);

        /// <summary>
        /// Returns a new instance of the <see cref="ActivationKeyManager"/> using base32 encoding.
        /// </summary>
        public static ActivationKeyManager Base32Manager => new ActivationKeyManager(PrintableEncoding.Base32);

         /// <summary>
        /// Returns a new instance of the <see cref="ActivationKeyManager"/> using base64 encoding.
        /// </summary>
        public static ActivationKeyManager Base64Manager => new ActivationKeyManager(PrintableEncoding.Base64);

        // Checks whether the file name is correct and, if necessary, whether the file exists.
        private static void ValidateFileName(string fileName, bool exists = false)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), GetResourceString("ArgumentNull_Path"));
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentException(GetResourceString("Argument_PathEmpty"), nameof(fileName));
            if (IsInvalidPath(fileName))
                throw new ArgumentException(GetResourceString("Argument_InvalidPathChars"));
            if (exists && !File.Exists(fileName))
                throw new FileNotFoundException(GetResourceString("IO.FileNotFound_FileName", fileName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyManager"/> using the default parameters.
        /// </summary>
        public ActivationKeyManager()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyManager"/> using the specified parameters.
        /// </summary>
        /// <param name="header">The header of the binary activation key data, which is used to verify the file format.</param>
        /// <param name="encoding">Encoding used to encode/decode activation key data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public ActivationKeyManager(ushort header, IPrintableEncoding encoding, params char[] delimiters)
        {
            _textParser = new ActivationKeyTextParser(encoding, delimiters);
            _binaryParser = new ActivationKeyBinaryParser(header);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyManager"/> using the specified parameters.
        /// </summary>
        /// <param name="encoding">Encoding used to encode/decode activation key data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public ActivationKeyManager(IPrintableEncoding encoding, params char[] delimiters)
        {
            _textParser = new ActivationKeyTextParser(encoding, delimiters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyManager"/> using the specified parameters.
        /// </summary>
        /// <param name="header">The header of the binary activation key data, which is used to verify the file format.</param>
        /// <param name="encoding">Encoding used to encode/decode activation key data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public ActivationKeyManager(ushort header, PrintableEncoding encoding, params char[] delimiters)
        {
            _textParser = new ActivationKeyTextParser(encoding, delimiters);
            _binaryParser = new ActivationKeyBinaryParser(header);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyManager"/> using the specified parameters.
        /// </summary>
        /// <param name="encoding">Encoding used to encode/decode activation key data.</param>
        /// <param name="delimiters">An array of characters that are used to split the string into parts of the activation key.</param>
        public ActivationKeyManager(PrintableEncoding encoding, params char[] delimiters)
        {
            _textParser = new ActivationKeyTextParser(encoding, delimiters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyManager"/> using the specified parameters.
        /// </summary>
        /// <param name="header">The header of the binary activation key data, which is used to verify the file format.</param>
        public ActivationKeyManager(ushort header)
        {
            _binaryParser = new ActivationKeyBinaryParser(header);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyManager"/> using the specified parsers.
        /// </summary>
        /// <param name="textParser">The text parser used to parse activation key data.</param>
        /// <param name="binaryParser">The binary parser used to parse activation key data.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ActivationKeyManager(ActivationKeyTextParser textParser, ActivationKeyBinaryParser binaryParser)
        {
            if (textParser == null)
                throw new ArgumentNullException(nameof(textParser), GetResourceString("ArgumentNull_WithParamName", nameof(textParser)));
            if (binaryParser == null)
                throw new ArgumentNullException(nameof(binaryParser), GetResourceString("ArgumentNull_WithParamName", nameof(binaryParser)));

            _textParser = textParser;
            _binaryParser = binaryParser;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyManager"/> using the specified parser.
        /// </summary>
        /// <param name="binaryParser">The binary parser used to parse activation key data.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ActivationKeyManager(ActivationKeyBinaryParser binaryParser)
        {
            if (binaryParser == null)
                throw new ArgumentNullException(nameof(binaryParser), GetResourceString("ArgumentNull_WithParamName", nameof(binaryParser)));

            _binaryParser = binaryParser;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationKeyManager"/> using the specified parser.
        /// </summary>
        /// <param name="textParser">The text parser used to parse activation key data.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ActivationKeyManager(ActivationKeyTextParser textParser)
        {
            if (textParser == null)
                throw new ArgumentNullException(nameof(textParser), GetResourceString("ArgumentNull_WithParamName", nameof(textParser)));

            _textParser = textParser;
        }

        /// <summary>
        /// Reads an activation key from the specified file.
        /// </summary>
        /// <param name="fileName">Path to the file containing the activation key.</param>
        /// <param name="binary">Flag indicating that the file contains binary data.</param>>
        /// <returns>An instance of the <see cref="ActivationKey"/>parsed from the file.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="fileName"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="fileName"/> is empty or contains only whitespace characters,
        /// or if fileName contains invalid characters.</exception>
        /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
        public ActivationKey LoadFromFile(string fileName, bool binary = true)
        {
            ValidateFileName(fileName, true);

            return binary
                ? _binaryParser.Parse(File.OpenRead(fileName))
                : _textParser.Parse(File.OpenRead(fileName));
        }

        /// <summary>
        /// Reads an activation key from the specified file.
        /// </summary>
        /// <param name="fileName">Path to the file containing the activation key.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>An instance of the <see cref="ActivationKey"/>parsed from the file.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="fileName"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="fileName"/> is empty or contains only whitespace characters,
        /// or if fileName contains invalid characters.</exception>
        /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
        public ActivationKey LoadFromFile(string fileName, Encoding encoding)
        {
            ValidateFileName(fileName, true);

            return _textParser.Parse(File.ReadAllText(fileName, encoding ?? Encoding.UTF8));
        }

        /// <summary>
        /// Reads an activation key from the ini file.
        /// </summary>
        /// <param name="fileName">Path to the file containing the activation key.</param>
        /// <param name="section">Ini file section where the activation key is located.</param>
        /// <param name="key">Ini file key where the activation key is located.</param>
        /// <param name="encoding">The encoding applied to the contents of the ini file.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> parsed from the ini file.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="fileName"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="fileName"/> is empty or contains only whitespace characters,
        /// or if fileName contains invalid characters.</exception>
        /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
        public ActivationKey LoadFromIniEntry(string fileName, string section, string key = null, Encoding encoding = null)
        {
            ValidateFileName(fileName, true);

            return _textParser.Parse(IniFile.GetEntry(fileName, encoding, section, key));
        }

        /// <summary>
        /// Reads multiple ActivationKeys from an ini file.
        /// </summary>
        /// <param name="fileName">Path to the file containing the activation key.</param>
        /// <param name="section">Ini file section where the activation key is located.</param>
        /// <param name="key">Ini file keys where the activation key is located.</param>
        /// <param name="encoding">The encoding applied to the contents of the ini file.</param>
        /// <returns>An array of <see cref="ActivationKey"/> instances parsed from the ini file.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="fileName"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="fileName"/> is empty or contains only whitespace characters,
        /// or if fileName contains invalid characters.</exception>
        /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
        public ActivationKey[] LoadFromIniEntries(string fileName, string section, string key = null, Encoding encoding = null)
        {
            ValidateFileName(fileName, true);

            List<ActivationKey> keys = new List<ActivationKey>();
            foreach (string value in IniFile.GetEntries(fileName, encoding, section, key))
            {
                if (value.IsNullOrEmpty())
                    continue;

                keys.Add(_textParser.Parse(value));
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Reads the activation key from the Windows registry.
        /// </summary>
        /// <param name="hive">The registry hive where the activation key is located.</param>
        /// <param name="key">The registry key where the activation key is located.</param>
        /// <param name="parameter">The registry parameter that contains the activation key.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> parsed from the registry.</returns>
        /// <exception cref="InvalidDataException">If the registry value does not match the expected data type.</exception>
        /// <exception cref="SecurityException">The user does not have the permissions required to read from the registry key.</exception>
        /// <exception cref="IOException">The registry key contains the specified value has been marked for deletion.</exception>
        /// <exception cref="UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        public ActivationKey LoadFromRegistry(RegistryHive hive, string key, string parameter)
        {
            if (key.IsNullOrWhiteSpace())
                throw new ArgumentException(GetResourceString("Argument_EmptyPath"), nameof(key));

            switch (GetRegistryValue(hive, key, parameter, null))
            {
                case null:
                    throw new ArgumentException(GetResourceString("Arg_RegKeyNotFound"), nameof(key));
                case string s:
                    return _textParser.Parse(s);
                case byte[] bytes:
                    return _binaryParser.Parse(bytes);
                default:
                    throw new InvalidDataException(GetResourceString("Arg_RegBadKeyKind"));
            }
        }

        /// <summary>
        /// Reads the activation key from the Windows registry.
        /// </summary>
        /// <param name="key">The registry key where the activation key is located.</param>
        /// <param name="parameter">Registry parameter that contains the activation key.</param>
        /// <returns>An instance of <see cref="ActivationKey"/> parsed from the registry.</returns>
        /// <exception cref="InvalidDataException">If the registry value does not match the expected data type.</exception>
        /// <exception cref="SecurityException">The user does not have the permissions required to read from the registry key.</exception>
        /// <exception cref="IOException">The registry key contains the specified value has been marked for deletion.</exception>
        /// <exception cref="UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        public ActivationKey LoadFromRegistry(string key, string parameter)
        {
            if (key.IsNullOrWhiteSpace())
                throw new ArgumentException(GetResourceString("Argument_EmptyPath"), nameof(key));

            switch (GetRegistryValue( key, parameter, null))
            {
                case null:
                    throw new ArgumentException(GetResourceString("Arg_RegKeyNotFound"), nameof(key));
                case string str:
                    return _textParser.Parse(str);
                case byte[] bytes:
                    return _binaryParser.Parse(bytes);
                default:
                    throw new InvalidDataException(GetResourceString("Arg_RegBadKeyKind"));
            }
        }

        /// <summary>
        /// Writes an activation key to the specified file.
        /// </summary>
        /// <param name="activationKey">Activation key to be saved.</param>
        /// <param name="fileName">Path to save the activation key.</param>
        /// <param name="encoding">The encoding applied to the contents of the text file.</param>
        /// <exception cref="ArgumentNullException">If the <paramref name="fileName"/> parameter is null.</exception>
        public void SaveToFile(ActivationKey activationKey, string fileName, Encoding encoding = null)
        {
            if (activationKey == null)
                throw new ArgumentNullException(nameof(activationKey),
                    GetResourceString("ArgumentNull_WithParamName", nameof(activationKey)));
            ValidateFileName(fileName);

            File.WriteAllText(fileName, _textParser.GetString(activationKey), encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// Writes an activation key to the specified file.
        /// </summary>
        /// <param name="activationKey">Activation key to be saved.</param>
        /// <param name="fileName">Path to save the activation key.</param>
        /// <param name="binary">Flag indicating that the file contains binary data.</param>>
        /// <returns>An instance of the <see cref="ActivationKey"/>parsed from the file.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="fileName"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="fileName"/> is empty or contains only whitespace characters,
        /// or if fileName contains invalid characters.</exception>
        public void SaveToFile(ActivationKey activationKey, string fileName, bool binary)
        {
            if (activationKey == null)
                throw new ArgumentNullException(nameof(activationKey),
                    GetResourceString("ArgumentNull_WithParamName", nameof(activationKey)));
            ValidateFileName(fileName);

            using (Stream stream = File.Create(fileName))
            {
                if (binary)
                    _binaryParser.Write(activationKey, stream);
                else
                    _textParser.Write(activationKey, stream);
            }
        }

        /// <summary>
        /// Writes an activation key to the Windows registry.
        /// </summary>
        /// <param name="activationKey">Activation key to be saved.</param>
        /// <param name="hive">The registry hive where the activation key is located.</param>
        /// <param name="key">The registry key where the activation key is located.</param>
        /// <param name="parameter">The registry parameter that contains the activation key.</param>
        /// <param name="binary">Flag indicating that the file contains binary data.</param>
        /// <exception cref="ArgumentNullException">One of arguments is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="key"/> parameter is empty.</exception>
        /// <exception cref="SecurityException">The user does not have the permissions required to read from the registry key.</exception>
        /// <exception cref="IOException">The registry key contains the specified value has been marked for deletion.</exception>
        /// <exception cref="UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        public void SaveToRegistry(ActivationKey activationKey, RegistryHive hive, string key, string parameter, bool binary = true)
        {
            if (activationKey == null)
                throw new ArgumentNullException(nameof(activationKey),
                    GetResourceString("ArgumentNull_WithParamName", nameof(activationKey)));
            if (key.IsNullOrWhiteSpace())
                throw new ArgumentException(GetResourceString("Argument_EmptyPath"), nameof(key));

            if (binary)
                SetRegistryValue(hive, key, parameter, _binaryParser.GetBytes(activationKey), RegistryValueKind.Binary);
            else
                SetRegistryValue(hive, key, parameter, _textParser.GetString(activationKey), RegistryValueKind.String);
        }

        /// <summary>
        /// Writes an activation key to the Windows registry.
        /// </summary>
        /// <param name="activationKey">Activation key to be saved.</param>
        /// <param name="key">The registry key where the activation key is located.</param>
        /// <param name="parameter">The registry parameter that contains the activation key.</param>
        /// <param name="binary">Flag indicating that the file contains binary data.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SecurityException">The user does not have the permissions required to read from the registry key.</exception>
        /// <exception cref="IOException">The registry key contains the specified value has been marked for deletion.</exception>
        /// <exception cref="UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        public void SaveToRegistry(ActivationKey activationKey, string key, string parameter, bool binary = true)
        {
            if (activationKey == null)
                throw new ArgumentNullException(nameof(activationKey),
                    GetResourceString("ArgumentNull_WithParamName", nameof(activationKey)));
            if (key.IsNullOrWhiteSpace())
                throw new ArgumentException(GetResourceString("Argument_EmptyPath"), nameof(key));

            if (binary)
                SetRegistryValue(key, parameter, _binaryParser.GetBytes(activationKey), RegistryValueKind.Binary);
            else
                SetRegistryValue(key, parameter, _textParser.GetString(activationKey), RegistryValueKind.String);
        }
    }
}