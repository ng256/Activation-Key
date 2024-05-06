# Activation Key 2.0 Project.

## Contents.  

1. [Overview](#overview)
2. [Format.](#format)
3. [Futures.](#futures)
4. [Key binding.](#key-binding)
5. [Usage.](#usage)
6. [Details.](#details)


## Overview.

### Protecting  software with an activation key.

In the modern world, software protection is a pressing task for developers. One of the effective ways to protect client software from unauthorized use and distribution is to use an activation key, also known as a license key, product key, or software key. In this article, we will look at the process of creating an activation key, which uses environment variables to bind to the end workstation's firmware and encrypt the data using various cryptographic algorithms. This will ensure reliable protection of the generated activation keys and prevent an attacker from falsifying them.

An activation key is a unique special software identifier that confirms that a copy of the program was obtained legally, since only the official publisher, having a key generator and knowing the secret parameters, can create and provide such a key to the end user. This approach can be used to solve various problems, such as limiting the use of a program for a certain time, preventing illegal distribution on unregistered workstations, managing user accounts using login and password, and other tasks related to the implementation of security policies in various applications and systems.

### Creation and verification of activation keys.
License verification is an important aspect of software security. Most often, the license comes down to a code that verifies the validity of the activation key. In the simplest case, license verification can be performed by simply comparing the key with a previously known value. However, this approach does not provide sufficient protection against unauthorized use. A more reliable way is to use various data encryption and hashing algorithms. The algorithm must be reliable enough to prevent key forgery and unauthorized access to software functionality. In this case, the key can be encrypted, and license verification is performed using decryption. Also, when verifying a key, a checksum is calculated based on data provided by the user and application parameters kept secret. The calculated checksum is compared with the checksum stored in the key. If the checksums match, the key is considered valid. As an additional (optional) condition for the key to be relevant, it is possible to specify the expiration date of its validity. In addition, the key may contain encrypted data that can be recovered during the key verification process. This approach allows you to provide the application with some information without which its functioning is impossible and helps make it difficult to reverse engineer the application.

### Using an activation key.

The implementation of key verification is usually done with a function that determines whether the supplied key is valid. If the key meets all the requirements, the function returns true, allowing the user to launch the application. Otherwise, the client software may display a warning or deny access to the application. This mechanism works based on pre-defined conditions for the validity of the license key and ensures that it is uniquely matched to the software.
In more complex security models, key verification may be coupled with decryption of the application binary. Only valid keys allow you to decrypt a file necessary for the application to launch and function correctly if they contain, for example, the password with which the file was encrypted.

### ActivationKey class.

The article introduces the ActivationKey class, which is a tool for creating, validating and managing activation keys. The class contains methods for reading and writing activation keys, creating and validating keys using various encryption and hashing algorithms, and extracting data from the encrypted portion of the key. In this case, the activation key is a set of data, and a hash function is used to calculate the checksum of this data. When a key is verified, a checksum is calculated based on the data provided by the user and compared with the checksum stored in the key. If the checksums match, the key is considered valid. As an additional (optional) condition for the key to be relevant, it is possible to specify the expiration date of its validity.

The ActivationKey class can be used in various projects to protect client software. It provides developers with a convenient tool for creating and managing activation keys, which allows them to ensure reliable software protection from unauthorized use.

The peculiarity of this tool is that it contains methods for generating a cryptographic key based on the specified binding to hardware and software. An additional feature is the ability to embed any information directly into the key. This information can be recovered as a byte array during key verification. The key can be stored in human-readable text so that it can be easily transmitted to the end user. This approach is intended to make it difficult for cracker to reverse engineer an application to bypass key verification.

## Format.  

During development, the optimal key format was chosen: DATA-HASH-SEED. 

For example, KCATBZ14Y-VGDM2ZQ-ATSVYMI.

The key format was specially selected in such a way as to ensure readability in text representation, avoiding uncorrect interpretation of symbols, and also, if possible, reduce its length while maintaining cryptographic strength. The key format was specially selected in such a way as to ensure readability in text representation, avoiding erroneous interpretation of symbols, and also, if possible, reduce its length while maintaining cryptographic strength. This was achieved through the use of special algorithms for encryption, hash calculations and text encoding of data. We'll talk about these algorithms later. 

The key consists of several parts, separated by a special symbol to facilitate parsing, the meaning of which is hidden from the end user and understandable only to the application. The table below shows the name and purpose of these parts.

| Part | Description |
| :----: | :---- |
| Data | A part of the key encrypted with a password. Contains the key expiration date and application options. |
| Hash | Checksum of the key expiration date, password, options and environment parameters. |
| Seed | Initialization vector that used to decode the data. |


## Futures.  

ActivationKey provides the following capabilities to automate the creation, storage and conversion of the activation key:

- Generating many unique activation keys and checking them.
- Storage and recovery of encrypted secret application data directly in the activation key.
- Providing special objects binary reader and text reader for reading decrypted data in text or binary form.
- Use of built-in or specified encryption and hashing algorithms.
- A wide selection of tools for converting a key into text or binary formats, as well as methods for obtaining a key from different file formats, the Windows registry, data streams, string variables and byte arrays.

## Key binding.  

Activation key is generated and verified using the following parameters:
- **expiration date** - limits the program's validity to the specified date. If value is ommited, it does not expire.  
- **data** - information that is restored when checking the key in its original form; may contain data such as the maximum number of launches, a key for decrypting a program block, restrictions and permisions to use any functions and other parameters necessary for the correct operation of the program. A value null for this parameter, when validated, will return an empty byte array.   
- **environment** - parameters for binding to the environment. These may include the name and version of the application, workstation ID, username, MAC address, etc. If you do not specify environment parameters, then the key will not take any bounds.  

Thus, a range of tasks is solved:
- limiting the period of use of the program;
- limiting the distribution of the program to other computers;
- accounting of usernames and passwords;
- differentiation of user access rights to various program functions;
- storage in the key important information, without which application launch is impossible, for example you can add an cryptographyc token for encrypted assembly.

It is also possible to create a key without any limits.

## Usage.  
  
In order to use the capabilities of the activation key project, you must add a link to the **ActivationKey.dll** libraries and declare using the namespace:

```csharp
using System.Security.Activation;
```
- _THIS PARAGRAPH IS UNDER CONSTRUCTION..._

## Details.

- _THIS PARAGRAPH IS UNDER CONSTRUCTION..._
  
### Briefly about embeded classes.   

| Class | Description |  
| :---: | :-------- |  
| ARC4 | Fork of [RC4](https://en.wikipedia.org/wiki/RC4) cryptography provider designed by Ron Rivest © for encrypt/decrypt the data part. |  
| SipHash | Fork of [SipHash](https://en.wikipedia.org/wiki/SipHash) algorithm add–rotate–xor based family of pseudorandom functions created by Jean-Philippe Aumasson and Daniel J. Bernstein. © |  
| Base32 | Fork of [ZBase-32](https://github.com/denxc/ZBase32Encoder) numeral system data to string encoder designed by Denis Zinchenko © for text key representation. |  
| CustomBase | Fork of [BaseNcoding](https://github.com/KvanTTT/BaseNcoding) - another algorithm for binary data to string encoding by KvanTTT ©. |  

You can replace them with your own implementation of encryption, hash calculation and encoding of data.

[↑ Back to contents.](#contents)
