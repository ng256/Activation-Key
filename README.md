# Activation Key 2.0 .NET Class Library

## Contents.  

1. [Overview](#overview)
2. [Details.](#details)
3. [Usage.](#usage)

## Overview.

### Protecting  software with an activation key.

Software protection is an important aspect for developers. One effective way to protect client software from unauthorized use and distribution is to use an activation key, also known as a license key, product key, or software key. In this article we will look at the process of creating an activation key, which uses environment variables to bind to the identifier of the end workstation, and also encrypts data using various cryptographic algorithms. This will ensure reliable protection of the generated activation keys and will not allow an attacker to forge them.

An activation key is a unique special software identifier that confirms that a copy of the program was obtained legally, since only the official publisher, having a key generator and knowing the secret parameters, can create and provide such a key to the end user. This approach can be used to solve various problems, such as limiting the use of a program for a certain time, preventing illegal distribution on unregistered workstations, managing user accounts using login and password, and other tasks related to the implementation of security policies in various applications and systems.

### Creation and verification of activation keys.
Most often, the it comes down to a code that verifies the validity of the activation key. In the simplest case, verification can be performed by simply comparing the key with a previously known value. However, this approach does not provide sufficient protection against unauthorized use.

A more reliable way is to use various data encryption and hashing algorithms. The algorithm must be reliable enough to prevent key forgery and unauthorized access to software functionality. In this case, the key can be encrypted, and license verification is performed using decryption. Also, when verifying a key, a checksum is calculated based on data provided by the user and application parameters kept secret. The calculated checksum is compared with the checksum stored in the key. If the checksums match, the key is considered valid. As an additional (optional) condition for the key to be relevant, it is possible to specify the expiration date of its validity.

The implementation of key verification is usually done with a function that determines whether the supplied key is valid. If the key meets all the requirements, the function returns true, allowing the user to launch the application. Otherwise, the client software may display a warning or deny access to the application. This mechanism works based on pre-defined conditions for the validity of the activation key and ensures that it is uniquely matched to the software.

In more complex security models, key verification may be coupled with decryption of the application binary file. Only valid keys allow you to decrypt a file necessary for the application to launch and function correctly if they contain, for example, the password with which the file was encrypted. This approach allows you to provide the application with some information without which its functioning is impossible and helps make it difficult to reverse engineer the application to bypass key verification.

### Library contents.

The project in question is a dll library that can be used in any solution.
The System.Security.Activation namespace includes an implementation of the ActivationKey class and other tools for working with it.
The System.Text namespace includes the IPrintableEncoding interface and the PrintableEncoding enumeration. They determine how the activation key is presented in text form.

## Details.

### ActivationKey class.

This project introduces the **ActivationKey** class, which is a tool for creating, validating and managing activation keys. The class contains methods for reading and writing activation keys, creating and validating keys using various encryption and hashing algorithms, and extracting data from the encrypted part of the key. In this case, the activation key is a set of data, and a hash function is used to calculate the checksum of this data. During the key verification process, a checksum is calculated using data provided by the user as well as predefined by the application, and then its value is compared with the checksum stored in the key. If the checksums match, the key is considered valid. As an additional (optional) condition for the key to be relevant, it is possible to specify the expiration date of its validity.

The ActivationKey class can be used in various projects to protect software. It provides developers with a convenient tool for creating and managing activation keys, which allows them to ensure reliable software protection from unauthorized use.

A special feature of this tool is that it contains methods for generating a cryptographic key based on the specified binding to hardware and software (so-called environment parameters). Another feature is the ability to set an expiration date for the key and to include any information directly within the key. This information can be recovered as a byte array during key verification. The key can be stored as human-readable text or in another format that allows for easy transmission to the end user.

### Format of the key.  

The designing of the optimal activation key format resulted in the following structure:

DATA-HASH-SEED.

For example, KCATBZ14Y-VGDM2ZQ-ATSVYMI.

The key format was specially selected in such a way as to ensure readability in text representation, avoiding incorrect interpretation of symbols, and also, if possible, reduce its length while maintaining cryptographic strength. This was achieved through the use of special algorithms for encryption, hash calculations and text encoding of data. We'll talk about these algorithms later, but for now let's take a closer look at the composition and purpose of each part of the key.

The key consists of several parts, separated by a special symbol to facilitate parsing, the meaning of which is hidden from the end user and understandable only to the application. The table below shows the name and purpose of these parts.

| Part | Description |
| :----: | :---- |
| Data | Content of encrypted expiration date and application data (optional). This embedded data can be recovered after successful key verification. |
| Hash | Checksum of key expiration date, encrypted application data, and environment identifiers. Ensures the validity of the key during verification. |
| Seed | The initialization value that was used to encrypt the data. Allows to generate unique keys every time to increase cryptographic strength. |

In fact, all parts of the key are essentially byte arrays converted to text representation using a special encoding that uses only printable characters. In simplified form, a class declaration looks like this:

```csharp
C#
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
### Futures.  

The main goal of this project is to provide the developer with mechanisms for generating keys, as well as making it easier to integrate them into a complete solution without having to worry about data conversion. The generator works with any number of input parameters, with any hash calculation and data encryption algorithms. Verifying the key takes a minimal amount of code in one line, which allows you to answer one question: is it possible to successfully activate this software using a given object containing the key?

Here is a short list of the general features:

- Generating many unique activation keys and checking them.
- Storing and recovering application secret data embedded directly in the activation key.
- Providing special objects binary reader and text reader for reading decrypted data in text or binary form.
- Use of built-in or specified encryption and hashing algorithms.

A lot of tools for converting a key into text or binary formats, as well as methods for obtaining a key from different file formats, the Windows registry, data streams, string variables and byte arrays. All these stuff were created specifically to automate the process of managing activation keys from creation to verification as transparently as possible, so that the software developer does not care about the form in which the key will be delivered to the end user and how it will be stored. Now let's talk about how all these futures were implemented.

### Generating a new key.

Activation key is generated and verified using the following parameters:
- **environment** - parameters for binding to the environment. These may include the name and version of the application, workstation ID, username, etc. If you do not specify environment parameters, then the key will not take any bounds.  
- **expiration date** - limits the program's validity to the specified date. If value is ommited, it does not expire.  
- **application data** - embedded information that is restored when checking the key in bytes; may contain data such as the maximum number of launches, a key for decrypting a program block, restrictions and permissions to use any functions and other parameters necessary for the correct operation of the program. A value null for this parameter, when validated, will return an empty byte array.  

**Important note about environment and data parameters!** Although the key generator accepts any objects, don't trust it too much. The internal serialize function works best with objects that support serialization. However, class serialization with BinaryFormatter is known to be deprecated. For security reasons, it's recommended to use only basic types such as numbers, strings, and fixed-length structures. This is not a strict requirement, but good advice to follow.

```csharp
string username;
byte[] macaddr;

// ...obtaining username and macaddr

object[] environment =      // Collected binding identifiers of any length
{
    // Static application parameters.
    "MyApp",                // Application title.
    1, 0, 					        // Application version.

    // Dynamic parameters.
    username ,              // Registered user.
    macaddr,                // MAC address of the network adapter.
}

DateTime expirationDate = DateTime.Now.AddMonths(1), // expiration date

object[] appData =  // Data that needs to be stored in the key
{
    0x73, 0x65, 0x63,       // Any secret numbers.
    0x72, 0x65, 0x74        // Decryption key for resource files maybe?
}
```

The **ActivationKey** class is responsible for creating a unique key that can be used to activate software or services. This class provides two key encryption methods:

1. A basic method that uses the built-in RC4 and SipHash custom modifications for working with activation keys and related data. The choice of these algorithms determines the creation of sufficiently reliable keys, the length of which is convenient for representation in text form.
```csharp
var key = ActivationKey.CreateEncryptor(environment).Generate(expirationDate, appData);
```
2. An advanced method that allows the user to specify specific encryption and hashing algorithms for encrypting data to the activation key. This method gives more flexibility in customizing the encryption process to meet the specific needs of the project. It is better to transfer such keys as a binary file.
```csharp
var key = ActivationKey.CreateEncryptor<AesManaged, MD5CryptoServiceProvider>(environment).Generate(expirationDate, appData);
```

### Obtaining of a previously generated key.
With the ActivationKeyManager class, you can effortlessly read activation keys from various sources and verify their validity. This class supports multiple formats:

- Plain text files
- Binary files
- INI files
- Windows registry entry (binary or text kind)
- Data streams
For example, if you have an application that requires an activation key for user authentication, you can utilize the ActivationKeyManager class to load the key from an INI file and validate it. If the key proves to be valid, you can proceed with using the application. However, if the key is invalid, you can display an error message and prompt the user to enter the correct key. All this can be accomplished with just a couple of lines of code.

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
      .LoadFromRegistry("HKEY_CURRENT_USER\SOFTWARE\MyApp\registration", "ActivationData")
      .Verify())
{
    // See previous example...
}
```

### Verifying the key.
However, enough talk about key creation and storage, as well as conversions to various formats. After all, the most important event in the life of an activation key is its verification. And the special class ActivationKeyDecryptor will help us to complete this task.

To create an instance of the ActivationKeyDecryptor class, you need to pass certain parameters to its constructor:

activationKey - the activation key that needs to be verified;
environment - the parameters we are already familiar with were used to generate a unique key. Important note! When passing environment parameters to the constructor, you must follow the same order as when creating an encryption device. Any differences in the number, order, or value of the parameters passed will cause the hash checksum to be different, which means the key will never pass the verification successfully.
The ActivationKey class also contains a method CreateDecryptor for quickly creating a decryptor.

The class attempts to decrypt the data contained in the key using a user-defined algorithm.
If the data is successfully decrypted, the class sets the Success property to true.
The class returns the decrypted data using the GetBinaryReader or GetTextReader methods.
Similar to the mentioned key encryption algorithms, this class implements two methods for decrypting the key:
1. A basic method that uses the built-in RC4 and SipHash custom modifications for working with activation keys and related data.
2. An advanced method that allows the user to specify specific encryption and hashing algorithms for decrypting data in the activation key.
   
```csharp
// Predifined example a key.
Activation key = "FVDZTMKGJXGZS-4FPHA5Y-UVNYMNY";

object[] environment =      // Collected binding identifiers of any length
{
    GetMagicNumbers(),      // Magic numbers. What would the world be like without them?
    "MyApp",      // Application name.
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
A few words about the Data property:

If the check fails, the property is null.
If no data has been stored in the key, the property will return an empty array.
If data has been stored in the key, then the property will contain that data as a byte array.
The ExpirationDate property returns the actual date when the activation key will expire. If the key was created without specifying an expiration date, the DateTime.MaxValue will be returned.

## Usage.  

Here's an example to illustrate the point. It's a simple console application that generates keys using various encryption algorithms and encoding methods. It also saves these keys to files in both text and binary formats.

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
![image](https://github.com/ng256/Activation-Key/assets/90511962/fd6b8aa0-d5db-44be-99d6-02eeaf331f92)


### Briefly about embeded classes.   

| Class | Description |  
| :---: | :-------- |  
| ARC4 | Fork of [RC4](https://en.wikipedia.org/wiki/RC4) cryptography provider designed by Ron Rivest © for encrypt/decrypt the data part. |  
| SipHash | Fork of [SipHash](https://en.wikipedia.org/wiki/SipHash) algorithm add–rotate–xor based family of pseudorandom functions created by Jean-Philippe Aumasson and Daniel J. Bernstein. © |  
| Base32 | Fork of [ZBase-32](https://github.com/denxc/ZBase32Encoder) numeral system data to string encoder designed by Denis Zinchenko © for text key representation. |  
| CustomBase | Fork of [BaseNcoding](https://github.com/KvanTTT/BaseNcoding) - another algorithm for binary data to string encoding by KvanTTT ©. |  

You can replace them with your own implementation of encryption, hash calculation and encoding of data.

[↑ Back to contents.](#contents)
