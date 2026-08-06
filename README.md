# Activation Key .NET Class Library

Represents the management tool for activation keys that are used to protect your application. These keys are also known as license keys, product keys, product activation, software keys, and serial numbers. It is a special software key for a computer program that certifies that the copy of the program has been obtained legally.

<p align="left">
  <a href="LICENSE">
    <img src="https://img.shields.io/badge/license-MIT-green.svg" alt="MIT License">
  </a>
  <a href="https://sourceforge.net/projects/activation-key/?pk_campaign=badge&pk_source=vendor">
    <img src="https://b.sf-syn.com/badge_img/3453387/oss-community-choice-white?achievement=oss-community-choice" height="40" alt="SourceForge Community Choice">
  </a>
</p>

# Contents.  

1. [Introduction](#introduction)
2. [Background.](#background)
3. [Usage.](#usage)
4. [Summary.](#summary)

# Introduction
Software protection is an important aspect for developers. One effective way to protect client software from unauthorized use and distribution is to use an activation key, also known as a license key, product key, or software key. In this article we will look at the process of creating an activation key, which uses environment variables to bind to the identifier of the end workstation, and also encrypts data using various cryptographic algorithms. This will ensure reliable protection of the generated activation keys and will not allow an attacker to forge them.

## Key Features
1. **Security**: Keys are generated using cryptographically secure random data to ensure uniqueness.
2. **Flexibility**: Customizable key lengths, separators, and group formats.
3. **Integrity**: Checksum validation ensures keys have not been tampered with.
4. **Ease of Use**: Simple API for integration into existing systems.

## Installation
1. Download the latest version of `ActivationKeyBin.zip` from the [releases page](https://github.com/ng256/Activation-Key/releases).
2. Extract `ActivationKey.dll` from the archive.
3. Add the DLL as a reference in your project.
4. Start using the library by importing the namespace.

## License
This project is distributed under the MIT license. You are free to use and modify it as needed.

## Library contents
This project is a DLL file that can be used in any solution.

- The **System.Security.Activation** namespace includes an implementation of the **ActivationKey** class and other tools for working with it.
- The **System.Text** namespace includes the **IPrintableEncoding** interface and the **PrintableEncoding** enumeration. They determine how the activation key is presented in text form.

I have also added a demo application - a key generator based on this project. It generates a key using on the entered password, application name, processor and network adapter identifiers and embeds the entered data.

![example](https://github.com/user-attachments/assets/436f151b-bf5f-4f00-864d-ee5db4e564ac "Here's an example of using the library in a sample project that generates keys" )

# Background

## Protecting  software with an activation key
An activation key is a unique special software identifier that confirms that a copy of the program was obtained legally, since only the official publisher, having a key generator and knowing the secret parameters, can create and provide such a key to the end user. This approach can be used to solve various problems, such as limiting the use of a program for a certain time, preventing illegal distribution on unregistered workstations, managing user accounts using login and password, and other tasks related to the implementation of security policies in various applications and systems.

## Creation and verification of activation keys
Most often, the it comes down to a code that verifies the validity of the activation key. In the simplest case, verification can be performed by simply comparing the key with a previously known value. However, this approach does not provide sufficient protection against unauthorized use.

A more reliable way is to use various data encryption and hashing algorithms. The algorithm must be reliable enough to prevent key forgery and unauthorized access to software functionality. In this case, the key can be encrypted, and license verification is performed using decryption. Also, when verifying a key, a checksum is calculated based on data provided by the user and application parameters kept secret. The calculated checksum is compared with the checksum stored in the key. If the checksums match, the key is considered valid. As an additional (optional) condition for the key to be relevant, it is possible to specify the expiration date of its validity.

The implementation of key verification is usually done with a function that determines whether the supplied key is valid. If the key meets all the requirements, the function returns true, allowing the user to launch the application. Otherwise, the client software may display a warning or deny access to the application. This mechanism works based on pre-defined conditions for the validity of the activation key and ensures that it is uniquely matched to the software.

In more complex security models, key verification may be coupled with decryption of the application binary file. Only valid keys allow you to decrypt a file necessary for the application to launch and function correctly if they contain, for example, the password with which the file was encrypted. This approach allows you to provide the application with some information without which its functioning is impossible and helps make it difficult to reverse engineer the application to bypass key verification.

## ActivationKey class
This project introduces the **ActivationKey** class, which is a tool for creating, validating and managing activation keys. The class contains methods for reading and writing activation keys, creating and validating keys using various encryption and hashing algorithms, and extracting data from the encrypted part of the key. In this case, the activation key is a set of data, and a hash function is used to calculate the checksum of this data. During the key verification process, a checksum is calculated using data provided by the user as well as predefined by the application, and then its value is compared with the checksum stored in the key. If the checksums match, the key is considered valid. As an additional (optional) condition for the key to be relevant, it is possible to specify the expiration date of its validity.

The **ActivationKey** class can be used in various projects to protect software. It provides developers with a convenient tool for creating and managing activation keys, which allows them to ensure reliable software protection from unauthorized use.

A special feature of this tool is that it contains methods for generating a cryptographic key based on the specified binding to hardware and software (so-called environment parameters). Another feature is the ability to set an expiration date for the key and to include any information directly within the key. This information can be recovered as a byte array during key verification. The key can be stored as human-readable text or in another format that allows for easy transmission to the end user.

### Format of the key
The designing of the optimal activation key format resulted in the following structure:

**DATA-HASH-SEED**.

For example, **KCATBZ14Y-VGDM2ZQ-ATSVYMI**.

The key format was specially selected in such a way as to ensure readability in text representation, avoiding incorrect interpretation of symbols, and also, if possible, reduce its length while maintaining cryptographic strength. This was achieved through the use of special algorithms for encryption, hash calculations and text encoding of data. We'll talk about these algorithms later, but for now let's take a closer look at the composition and purpose of each part of the key.

The key consists of several parts, separated by a special symbol to facilitate parsing, the meaning of which is hidden from the end user and understandable only to the application. The table below shows the name and purpose of these parts.

| Part | Description |
| :----: | :---- |
| Data | Content of encrypted *expiration date* and *application data* (optional).<br>This embedded data can be recovered after successful key verification. |
| Hash | Checksum of key *expiration date*, encrypted *application data*, and *environment* identifiers.<br>Ensures the validity of the key during verification. |
| Seed | The initialization value that was used to encrypt the data.<br>Allows to generate unique keys every time to increase cryptographic strength. |

In fact, all parts of the key are essentially byte arrays converted to text representation using a special encoding that uses only printable characters. In simplified form, a class declaration looks like this:

```csharp
public class ActivationKey
{
    public byte[] Data;
    public byte[] Hash;
    public byte[] Seed;

    public override ToString()
    {
        // Return a string in the format "data-hash-seed".
    }
}
```
### Futures
The main goal of this project is to provide the developer with mechanisms for generating keys, as well as making it easier to integrate them into a complete solution without having to worry about data conversion. The generator works with any number of input parameters, with any hash calculation and data encryption algorithms. Verifying the key takes a minimal amount of code in one line, which allows you to answer one question: is it possible to successfully activate this software using a given object containing the key?

Here is a short list of the general features:

- Generating many unique activation keys and checking them.
- Storing and recovering application secret data embedded directly in the activation key.
- Providing special objects binary reader and text reader for reading decrypted data in text or binary form.
- Use of built-in or specified encryption and hashing algorithms.

A lot of tools for converting a key into text or binary formats, as well as methods for obtaining a key from different file formats, the Windows registry, data streams, string variables and byte arrays.

All these stuff were created specifically to automate the process of managing activation keys from creation to verification as transparently as possible, so that the software developer does not care about the form in which the key will be delivered to the end user and how it will be stored. Now let's talk about how all these futures were implemented.

### Cryptography
Most key generation examples follow a similar pattern: they introduce a function with a clever algorithm that includes a "magic numbers". This function takes certain parameters as input and returns a string based on those parameters. You cannot find such tricks here. This article is more of a guide to automating activation key management than a DIY cryptography tutorial. Despite the fact that the project still includes the implementation of crypto providers that the author considers the most effective for these purposes, he will not even list their source code here. This is not the main focus, and there are reasons for this.

Firstly, a negative aspect of .NET assemblies is their relatively easy ability to be decompiled. This issue goes beyond the problem of managing activation keys and requires additional steps to obfuscate the assembly code. Therefore, it will not be possible to keep the algorithm secret: any "cool-hacker" can identify the crypto function by simply downloading .NET Reflector or similar, and feel like a guru. Yep, I’ve done this myself a hundred times too.

Secondly, why reinvent the wheel if there are already quite a few established cryptographic algorithms that have been proven effective by experts? The built-in encryption algorithm was very popular in its time, but the main feature of the **ActivationKey** class is that it is adapted to generate instances using any nested algorithms that inherits the **SymmetricAlgorithm** and **HashAlgorithm** in the **System.Security.Cryptography** namespace.

Using the default key generator:

```csharp
var key = ActivationKey.DefaultEncryptor.Generate();
```
Using a key generator based on a specified cryptographic providers (AES and MD5, for example):
```csharp
var key = ActivationKey
         .CreateEncryptor<AesManaged, MD5CryptoServiceProvider>()
         .Generate();
```

Although, if you want to, you can modify the library and suggest your own algorithm without making major changes to the code, the activation key will still work seamlessly with your algorithm, just like it would if it was created specifically for it. The most important thing for protecting is the encryption parameters and data. About them in the next paragraph. And the rest of the problems are taken care of by the **ActivationKey** class.

### Key binding
Okay, let's say we have decided on an algorithm that allows to create keys using encryption. How can we ensure that each unique key truly confirms the right to use the software only to its legal owner and make its secondary resale unsuitable? Generally, each licensed copy of software issued is guaranteed fair use within a single workstation that has a unique identifier. This identifier usually consists of such parameters as, for example, the serial number of the board processor, the MAC address of the network interface, software environment parameters, etc. In addition, the user name, title and version of the licensed application, and other application parameters can be used in as such an identifier. By combining the above, we can generate a key that will be relevant only in the place where all these conditions are met.

Activation key is generated and verified using the following parameters:

- **environment** - parameters for binding to the environment. These may include the name and version of the application, workstation ID, username, etc. If you do not specify environment parameters, then the key will not take any bounds.
- **expiration date** - limits the program's validity to the specified date. If value is omitted, it does not expire.
- **application data** - embedded information that is restored when checking the key in bytes; may contain data such as the maximum number of launches, a key for decrypting a program block, restrictions and permissions to use any functions and other parameters necessary for the correct operation of the program. A value null for this parameter, when validated, will return an empty byte array.

To visualize the difference between the environment and application data, consider the following illustration.
![example](https://github.com/user-attachments/assets/6d06a163-ae2f-480d-8b18-dc7d0e777a04)

This way allows you to provide an activation key for a specific user of your software on a particular computer. After the expiration date, this key will no longer provide access to the protected software.

Each time you click "Generate!", a different key is generated using a random seed. However, all these activation keys contain the same information about the environment and application data.

**Important note about environment and data parameters!** Although the key generator accepts any objects, don't trust it too much. The internal serialize function works best with objects that support serialization. However, class serialization with BinaryFormatter is known to be deprecated. For security reasons, it's recommended to use only basic types such as numbers, strings, and fixed-length structures. This is not a strict requirement, but good advice to follow.

Here is a recommended list of types that can be serialized safely:

| Type | Description |
| :---- | :---- |
| bool <br> char, string <br> All signed and unsigned integers of size 8, 16, 32, 64 bits or arrays thereof <br> Floating point numbers of 32, 64 and 128 bits length or arrays of them <br> Bytes and chars arrays | Primitive types that are serialized by the runtime |
| DateTime <br> SecureString <br> Stream | Some special types of serialization that are supported internally |
| IConvertible <br> IFormattable | Types that inherit the following interfaces and implement the ToString method |
| ValueType | Unmanaged types |

### Generating a new key
Here are the basic steps to creating a unique key:

- First, all input parameters of the **environment** are serialized into a byte array, and a **seed** is randomly obtained.
- An encryption key is created based on the input parameters and the seed value.
- At this stage, an encryptor is created that associated to the environment.
- Next, the data, as well as the expiration date, is then serialized.
- A byte array is created that contains the serialized data.
- The source data is encrypted using the selected encryption algorithm.
- A hash of the original data and seed value is created using the selected hashing function.
- Finally, a new instance of the **ActivationKey** is created that contains the encrypted **data**, the calculated **hash**, and a random **seed** value that is used to generate the key.

![diagram](https://github.com/user-attachments/assets/e3cb5b63-d2e3-485a-b3c1-f07913eab513)

The **ActivationKeyEncryptor** class is responsible for creating a unique key that can be used to activate software or services. This class provides two key encryption methods:

A basic method that uses the built-in RC4 and SipHash custom modifications for working with activation keys and related data. The choice of these algorithms determines the creation of sufficiently reliable keys, the length of which is convenient for representation in text form.
An advanced method that allows the user to specify specific encryption and hashing algorithms for encrypting data to the activation key. This method gives more flexibility in customizing the encryption process to meet the specific needs of the project. It is better to transfer such keys as a binary file.
Both methods work in a similar way and generate robust cryptographic activation keys that have essentially the same structure and can be encoded in different forms.

Here is the general principle of how the key generator works:
```csharp
private ICryptoTransform encryptor;
private HashAlgorithm hasher;
private byte[] seed;

// This method binds the generator to the environment.
void CreateEncryptor(SymmetricAlgorithm symmetricAlgorithm, 
    HashAlgorithm hashAlgorithm, params object[] environment)
{
    hasher = hashAlgorithm;
    seed = symmetricAlgorithm.IV;
    using (PasswordDeriveBytes deriveBytes = 
        new PasswordDeriveBytes(Serialize(environment), seed))
    {
        encryptor = symmetricAlgorithm.CreateEncryptor
             (deriveBytes.GetBytes(symmetricAlgorithm.KeySize / 8), seed);
    }
}

// This method generates a new key.
ActivationKey Generate(DateTime expirationDate, params object[] data)
{
    byte[] serializedData = Serialize(expirationDate, data);
    byte[] encryptedData = encryptor.TransformFinalBlock(serializedData, 0, serializedData.Length);
    byte[] hash = hasher.ComputeHash(Serialize(serializedData, seed));
    return new ActivationKey(encryptedData, hash, seed);
}
```

You may notice that the environment parameters are not displayed explicitly in the key, they serve only to initialize the encryptor and affect the hash calculation. This approach ensures transparency of the key generation process and reliably hides information about the method of its creation from the user.

The developer can implement obfuscation of substitution of environment parameters to make reverse engineering more difficult, while obfuscation of the key generator is unnecessary. As such parameters, as already mentioned, you can use both static information, such as the name and version of the application, and dynamic data, such as client equipment identifiers - username, motherboard serial number, processor model, network interface MAC address, etc.

```csharp
// Dynamic parameters.
string username;
byte[] macaddr;

// Obfuscated method that is used to obtain certain secret parameters.
// The longer the generated sequence, the better.
byte[] GetMagicNumbers()
{
    /* For example, let's calculate pi squared accurate to n digit.
       All means are good for this. 
       For example, you can intentionally make calculations more complex 
       or use code from third-party libraries by using DLL imports. 
       You can also use encrypted bytecode.
    */
}

// ...obtaining username and macaddr

object[] environment =      // Collected binding identifiers of any length
{
    GetMagicNumbers(),      // Magic numbers. What would the world be like without them?
    "MyApp",                // Application name.
    1, 0, 					// Version.
    username ,              // Registered user.
    macaddr,                // MAC address of the network adapter.
}

DateTime expirationDate = DateTime.Now.AddMonths(1), // expiration date

object[] data =             // Data that needs to be stored in the key
{
    0x73, 0x65, 0x63, 
    0x72, 0x65, 0x74    // Any secret numbers
}

// As I promised, just one line of code:
var key = ActivationKey.CreateEncryptor(environment).Generate(expirationDate, data);
```

Converting the key to other types
There are various methods for transferring activation keys from the publisher to the user for their intended use. This process involves passing data through communication networks, writing it to files, and adding it to the registry, among other things.

It is crucial to ensure that the representation of the activation key is converted into the desired format. The **ActivationKey** class supports different converting methods, which can be categorized into two groups: text and binary representation. These tasks are effectively accomplished using specially additional classes called **ActivationKeyTextParser** and **ActivationKeyBinaryParser**.

The **ActivationKeyBinaryParser** is a tool that helps you parse binary data that contains activation keys. It has methods for parsing these activation keys and for creating instances of the ActivationKey class. The activation key is shown as a sequence of bytes with the necessary header. This header is utilized to confirm the file format. Therefore, the activation key can be provided in the form of a file. The user needs to specify the path to this file when registering their copy of the application. This format is also suitable for storing the key in the Windows registry.

Here is the serialized activation key structure:

| Part | Description |
| :---- | :---- |
| Header | 2 bytes |
| Data length | 32-bit integer |
| Hash length | 32-bit integer |
| Seed length | 32-bit integer |
| Data	| byte array |
| Hash	| byte array |
| Seed	| byte array |

This is how the C# implementation of that would look like:

```csharp
// A required header indicating that the data is in the correct format.
const ushort BinaryHeader = 0x4B61;

// Writing the activation key to the stream.
void Write(ActivationKey activationKey, Stream stream)
{
    using(BinaryWriter writer = new BinaryWriter(stream))
    {
        writer.Write(BinaryHeader);
        writer.Write(activationKey.Data.Length);
        writer.Write(activationKey.Hash.Length);
        writer.Write(activationKey.Seed.Length);
        writer.Write(activationKey.Data);
        writer.Write(activationKey.Hash);
        writer.Write(activationKey.Seed);
    }
}

// Parsing the stream containing an activation key.
ActivationKey Parse(Stream stream)
{
    ActivationKey activationKey = new ActivationKey();
    
    using(BinaryReader reader = new BinaryReader(stream))
    {
        ushort header = reader.ReadUInt16();
        if (header != BinaryHeader)
        throw new Exception();
        int dataLength = reader.ReadInt32();
        int hashLength = reader.ReadInt32();
        int tailLength = reader.ReadInt32();
        activationKey.Data = reader.ReadBytes(dataLength);
        activationKey.Hash = reader.ReadBytes(hashLength);
        activationKey.Seed = reader.ReadBytes(tailLength);
    }

    return activationKey;
}
```

The **ActivationKeyTextParser** offers tools for working with text data that represents activation keys. These keys are sequences of characters separated by specific delimiters.

This class provides methods for parsing and generating instances of the **ActivationKey** class. The **ActivationKeyTextParser** has several constructors, allowing you to set up parser parameters. It also includes methods for parsing activation keys and creating **ActivationKey** instances. The Parse methods enable you to convert strings representing activation keys into **ActivationKey** instances. On the other hand, the **GetString** method returns the text representation of the activation key as a delimited string.

Example text parser implementation in C#:

```csharp
// Current printable encoding
IPrintableEncoding encoding;

// Characterthat used as delimiter between activation key parts.
char separator = '-';

string GetStringSafe(byte[] bytes)
{
    return bytes == null ? string.Empty : encoding.GetString(bytes);
}

// Converting the activation key to string.
string GetString(ActivationKey activationKey)
{
    return string.Format("{0}{3}{1}{3}{2}", new object[5]
    {
        this.GetStringSafe(activationKey.Data),
        this.GetStringSafe(activationKey.Hash),
        this.GetStringSafe(activationKey.Seed)
        separator,
    });
}

// Parsing string containing an activation key.
ActivationKey Parse(string input)
{
    ActivationKey activationKey = new ActivationKey();
     
    if (string.IsNullOrEmpty(input))
        return;
    string[] items = input.ToUpperInvariant().Split(separator);
    if (items.Length >= 3)
    {
        activationKey.Data = encoding.GetBytes(items[0]);
        activationKey.Hash = encoding.GetBytes(items[1]);
        activationKey.Seed = encoding.GetBytes(items[2]);
    }
    return activationKey;
}
```

Furthermore, the **ActivationKeyTextParser** contains methods for retrieving specific parts of the activation key, such as the data, hash, or seed. This enables developers to access the information within the activation key and utilize it for various purposes. The key in text format is convenient for sending via email and other online text services. The user can easily copy this key from the email and paste it into the text input field of the application. This way, the key can be stored in an ini file for future use.

Choosing a method for representing binary information in text will inevitably lead us to the topic of N-based encoding. This subject has been widely discussed online ([example1](https://github.com/KvanTTT/BaseNcoding), [example2](https://github.com/denxc/ZBase32Encoder), [example3](https://github.com/deniszykov/BaseN)), so we will provide a brief overview of the encoding methods used in this article.

The **ActivationKeyTextParser** class provides static methods for creating and obtaining an objects, which is a set of methods that allow you to convert binary data to a text representation. This can be useful for displaying binary data in a human-readable format or for transmitting binary data over text-based communication channels. You can use the default method or select one of the available options to convert data into characters. The **IPrintableEncoding** implementation can be used to convert binary data such as **ActivationKey** into a textual representation that can be easily read and understood. 

```csharp
public interface IPrintableEncoding : ICloneable
{
    string GetString(byte[] bytes);
    byte[] GetBytes(string s);
}
```

The GetString and GetBytes methods allow you to encode and decode data. They can be useful when working with binary data that needs to be presented in a human-readable format. This means that all data will be represented as characters that can be copied and pasted or entered manually and saved using a program such as "Notepad".

Here are the encodings supported by the ActivationKeyTextParser class:

- **Base32Encoding** - returns the 32-character encoding (used by default).
- **Base64Encoding** - returns the 64-character encoding without the trailing symbol.
- **DecimalEncoding** - returns the decimal encoding.
- **HexadeciamlEncoding** - returns the hexadecimal encoding.
- **GetEncoding(*string alphabet*)** - creates an instance of a custom encoding class based on the passed alphabet string if you want to generate keys using your own character set.
- **GetEncoding(*PrintableEncoding encoding*)** - returns the encoding determined by the PrintableEncoding enumeration.
Write your own version of the encoding that implements the IPrintableEncoding interface and pass it to the **ActivationKeyTextParser** class constructor.

```csharp
IPrintableEncoding _encoding;

public ActivationKeyTextParser(IPrintableEncoding encoding, params char[] delimiters)
{
    _encoding = encoding ?? Base32Encoding;
}
```

In the delimiters parameter you can pass all the characters that will be considered delimiters between the data, the hash and the initial value. In the delimiters parameter you can pass all the characters that will be considered delimiters between the data, the hash and the seed parts of the key. The default is a hyphen.

The most convenient encoding is base-32, since it's case-insensitive and consists only of numbers and Latin letters. I recommend using this encoding and not looking for other ways.

**ActivationKeyConverter** is a tool that helps you convert ActivationKey objects to other data types and vice versa. It is a subclass of the TypeConverter class and includes methods such as **CanConvertFrom**, **CanConvertTo**, **ConvertFrom** and **ConvertTo**. These methods allow you to change **ActivationKey** objects to strings and byte arrays and vice versa.

This converter can be helpful when you work with data of the ActivationKey type. For instance, you can use it when storing the data in a database, transferring it over a network, or showing it in a user interface.

### Obtaining of a previously generated key
With the ActivationKeyManager class, you can effortlessly read activation keys from various sources and verify their validity. This class supports multiple formats:

- Plain text files
- Binary files
- INI files
- Windows registry entry (binary or text kind)
- Data streams
For example, if you have an application that requires an activation key for user authentication, you can utilize the **ActivationKeyManager** class to load the key from an INI file and validate it. If the key proves to be valid, you can proceed with using the application. However, if the key is invalid, you can display an error message and prompt the user to enter the correct key. All this can be accomplished with just a couple of lines of code.

The code you need might look like this:

```csharp
if (!ActivationKey
      .DefaultManager
      .LoadFromIniEntry("settings.ini", "registration", "key")
      .Verify())
{
    // Displaying a message and closing the window.
    string message = "Your version is unregistered."
         +" Would you like to enter a valid activation key?";
    string caption = "Registration warning";
    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
    DialogResult result = MessageBox.Show(message, caption, buttons);
    
    if (result == System.Windows.Forms.DialogResult.No)
        this.Close();

    // Calling the method for entering the activation key...
}
```

Content of ini file:

```ini
[Registration] 
Key=FVDZTMKGJXGZS-4FPHA5Y-UVNYMNY 
Owner=John 
#...etc
```

Another example, if you want to read an activation key from the Windows registry:

```csharp
if (!ActivationKey
      .DefaultManager
      .LoadFromRegistry("HKEY_CURRENT_USER\SOFTWARE\MyApp\Registration", "ActivationData")
      .Verify())
{
    // See previous example...
}
```

Using the ActivationKey.CreateManager method, you can create a manager that uses custom settings to convert the activation key. Here is a list of these settings:

- Binary header,
- Printable encoding,
- Delimiter symbols

```csharp
var manager = ActivationKey.CreateManager(0x0123, PrintableEncoding.Base64, '-', ':', ' ', '\t');
```

The first character in the list of delimiters will be used when converting the activation key to a string.

### Verifying the key

However, enough talk about key creation and storage, as well as conversions to various formats. After all, the most important event in the life of an activation key is its verification. And the special class **ActivationKeyDecryptor** will help us to complete this task.

To create an instance of the **ActivationKeyDecryptor** class, you need to pass certain parameters to its constructor:

- activationKey - the activation key that needs to be verified;
- environment - the parameters we are already familiar with were used to generate a unique key.

**Important note!** When passing environment parameters to the constructor, you must follow the same order as when creating an encryption device. Any differences in the number, order, or value of the parameters passed will cause the hash checksum to be different, which means the key will never pass the verification successfully.

The **ActivationKey** class also contains a method CreateDecryptor for quickly creating a decryptor.

The class attempts to decrypt the data contained in the key using a user-defined algorithm.
If the data is successfully decrypted, the class sets the Success property to true.
The class returns the decrypted data using the GetBinaryReader or GetTextReader methods.
Similar to the mentioned key encryption algorithms, this class implements two methods for decrypting the key:

1. A basic method that uses the built-in [RC4](https://github.com/ng256/ARC4) and [SipHash](https://en.wikipedia.org/wiki/SipHash) custom modifications for working with activation keys and related data.
2. An advanced method that allows the user to specify specific encryption and hashing algorithms for decrypting data in the activation key.

```csharp
// Predifined example a key.
Activation key = "FVDZTMKGJXGZS-4FPHA5Y-UVNYMNY";

object[] environment =      // Collected binding identifiers of any length
{
    GetMagicNumbers(),      // Magic numbers (obfuscated method).
    "MyApp",                // Application name.
    1, 0,                   // Version.
    username ,              // Registered user.
    macaddr,                // MAC address of the network adapter.
}

// Here are two methods to verify the key.

// 1. Special decryptor that can verify the key and recover encrypted data.
using(ActivationKeyDecryptor decryptor = key.CreateDecryptor(environment))
{
    if(decryptor.Success)
        using(TextReader reader = decryptor.GetTextReader())
        {
            //Now we know what's there!
            string secret = reader.ReadToEnd();
        }
}

// 2. Just checking the key.
bool success = key.Verify(environment);
```

A few words about the **Data** property:

- If the check fails, the property is null.
- If no data has been stored in the key, the property will return an empty array.
- If data has been stored in the key, then the property will contain that data as a byte array.
The **ExpirationDate** property returns the actual date when the activation key will expire. If the key was created without specifying an expiration date, the **DateTime.MaxValue** will be returned.

# Usage
Let's look at an example to illustrate the point. Here is a simple console application that generates keys using various encryption algorithms and encoding methods. It also saves these keys to files in both text and binary formats.

```csharp
using System;
using System.IO;
using System.Linq;
using System.Text; 
using System.Security.Activation;
using System.Security.Cryptography;
using System.Net.NetworkInformation;

internal static class Program
{
     // Obtaining MAC address.
     byte[] macAddr =
        (
            from netInterface in NetworkInterface.GetAllNetworkInterfaces()
            where netInterface.OperationalStatus == OperationalStatus.Up
            select netInterface.GetPhysicalAddress().GetAddressBytes()
        ).FirstOrDefault();

    // Here's an example of custom encoding that uses numbers, 
    // latin and cyrillic characters, in both uppercase and lowercase.
    private static string Base128 = 
    "0123456789"+
    "QWERTYUIOPASDFGHJKLZXCVBNM"+
    "qwertyuiopasdfghjklzxcvbnm"+
    "ЙЦУКЕЁНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ"+
    "йцукеёнгшщзхъфывапролджэячсмитьбю";

    // Input data. No article can be written without these simple, sincere words.
    private static byte[] HelloWorld = 
    { 
      0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20,
      0x77, 0x6f, 0x72, 0x6c, 0x64, 0x21 
    };

    private static void Main(string[] args)
    {
        // Pass one: using the default encryptor without data.
        Console.WriteLine();
        Console.WriteLine("Default cryptography without data:");
        using (ActivationKey key = ActivationKey.CreateEncryptor(macAddr).Generate()
        {
            using (ActivationKeyDecryptor decryptor = key.CreateDecryptor(macAddr)
            {
                Console.WriteLine("Base10: \t" + key.ToString(PrintableEncoding.Decimal));
                Console.WriteLine("Base16: \t" + key.ToString(PrintableEncoding.Hexadecimal));
                Console.WriteLine("Base32: \t" + key);
                Console.WriteLine("Base64: \t" + key.ToString(PrintableEncoding.Base64));
                Console.WriteLine("Base128:\t" + key.ToString(ActivationKeyTextParser.GetEncoding(Base128)));
                ActivationKey.DefaultManager.SaveToFile(key, "key1.bin", true);  //binary
                ActivationKey.DefaultManager.SaveToFile(key, "key1.txt", false); // text
            }
        }

        // Pass two: using the default encryptor with data.
        Console.WriteLine();
        Console.WriteLine("Default cryptography with data:");
        using (ActivationKey key = ActivationKey.CreateEncryptor(macAddr).Generate(HelloWorld))
        {
            using (ActivationKeyDecryptor decryptor = key.CreateDecryptor(macAddr))
            {
                if (decryptor.Success && decryptor.Data.Length != 0)
                {
                    using (TextReader reader = decryptor.GetTextReader(null))
                    {
                        Console.WriteLine("The key content is: " + reader.ReadToEnd());
                    }
                }            
                Console.WriteLine("Base10: \t" + key.ToString(PrintableEncoding.Decimal));
                Console.WriteLine("Base16: \t" + key.ToString(PrintableEncoding.Hexadecimal));
                Console.WriteLine("Base32: \t" + key);
                Console.WriteLine("Base64: \t" + key.ToString(PrintableEncoding.Base64));
                Console.WriteLine("Base128:\t" + key.ToString(ActivationKeyTextParser.GetEncoding(Base128)));
                ActivationKey.DefaultManager.SaveToFile(key, "key2.bin", true);  // binary
                ActivationKey.DefaultManager.SaveToFile(key, "key2.txt", false); // text
            }
        }

        // Pass three: using the AES encryptor and MD5 hash algorithm with data.
        Console.WriteLine();
        Console.WriteLine("Custom cryptography (AES+MD5) with data:");
        using (ActivationKey key = ActivationKey
            .CreateEncryptor<AesManaged, MD5CryptoServiceProvider>(macAddr)
            .Generate(HelloWorld))
        {
            using (ActivationKeyDecryptor decryptor = 
                key.CreateDecryptor<AesManaged, MD5CryptoServiceProvider>(macAddr))
            {
                if (decryptor.Success && (decryptor.Data.Length != 0))
                {
                    using (TextReader reader = decryptor.GetTextReader(null))
                    {
                        Console.WriteLine("The key content is: " + reader.ReadToEnd());
                    }
                }
                Console.WriteLine("Base10: \t" + key.ToString(PrintableEncoding.Decimal));
                Console.WriteLine("Base16: \t" + key.ToString(PrintableEncoding.Hexadecimal));
                Console.WriteLine("Base32: \t" + key);
                Console.WriteLine("Base64: \t" + key.ToString(PrintableEncoding.Base64));
                Console.WriteLine("Base128:\t" + key.ToString(ActivationKeyTextParser.GetEncoding(Base128)));
                ActivationKey.DefaultManager.SaveToFile(key, "key3.bin", true);  // binary
                ActivationKey.DefaultManager.SaveToFile(key, "key3.txt", false); // text
            }
        }
        Console.ReadKey();
    }
}
```

Console output:

![image](https://github.com/user-attachments/assets/33d8f9c9-a37d-484a-a7c8-cb9363ae6cf9)

Note the difference in key lengths for the default streaming algorithm RC4 and the AES block cipher, with data and without! Therefore, the use of long keys is more suitable for binary files than text.

# Summary
In conclusion, I would like to make a brief summary.

There is no need to obfuscate the key generation method and invent your own encryption algorithms. Instead, you should focus on the following tasks:

1. Complicate the code responsible for obtaining the parameters for binding the key to the environment.
2. Embed data into the key that allows you to decrypt binary files with executable code or application resources and other data critical for startup.
3. Use known printable encodings to represent the key in a convenient form for transfering.
4. Use algorithms that will allow you to generate short keys sufficient to achieve your goals without losing cryptographic strength.

The project described in this article can be used in various applications without any changes from version to version. To create unique keys, you need to pass different parameters that are relevant to the current situation.

## P. S. Briefly about embeded classes.

| Class  | Description |
| :----: | :---------- |
| ARC4 | Custom implementation of RC4 cryptography provider designed by Ron Rivest © for encrypt/decrypt the data part. |
| SipHash | 32-bit implementation of SipHash algorithm add–rotate–xor based family of pseudorandom functions created by Jean-Philippe Aumasson and Daniel J. Bernstein. © |
| Base32 | Fork of ZBase-32 numeral system data to string encoder designed by Denis Zinchenko © for text key representation. |
| CustomEncoding | Fork of BaseNcoding - another algorithm for binary data to string encoding by KvanTTT ©. |

[↑ Back to contents.](#contents)
