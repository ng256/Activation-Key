# Activation Key 1.1 Project.

## Contents.  

1. [Description.](#description)
2. [Format.](#format)
3. [Futures.](#futures)
4. [Key binding.](#key-binding)
5. [Usage.](#usage)
6. [Details.](#details)

## Description.  

Represents the activation key used to protect your C# application. It is also called a license key, product key, product activation, software key, and even a serial number. This is a special software key for a computer program. It certifies that the copy of the program was obtained legally. The generated keys help in solving such problems as: limiting the use of your program over time, preventing illegal distribution to unregistered workstations, managing user accounts using a login and password, and others.

License verification is an important aspect of software security. Most often, a license comes down to a code that verifies the validity of the activation key. In the simplest case, license verification can be performed by simply comparing the key with a previously known value. However, this approach does not provide sufficient protection against unauthorized use. A more reliable way is to use cryptographic algorithms to verify the key. In this case, the key can be encrypted, and license verification is performed using decryption.

Various algorithms and methods can be used to verify a license. One popular method is to use hash functions. In this case, the license is a set of data, and a hash function is used to calculate a checksum of that data. When a license is verified, a checksum is calculated based on the data provided by the user and compared with the checksum stored in the application. If the checksums match, then the license is considered valid.

A special feature of this tool is that it contains methods for generating a cryptographic key based on the specified hardware and software binding. An additional feature is the ability to embed any information directly into the key. This information can be recovered as a byte array during key verification.
The key can be stored as human-readable text so that it can be easily transmitted to the end user.

The implementation of key validation is typically done using the **Verify()** function, which determines whether the supplied key is valid. If the key meets all the requirements, the function returns **true**, allowing the user to launch the application. Otherwise, a warning or access denial appears. This mechanism works based on pre-defined conditions for the validity of the license key and ensures that it is uniquely matched to the software.

In more complex security models, key verification may be coupled with decryption of the application binary. Only valid licenses have the ability to "decrypt" the file, which is the equivalent of the **GetOptions()** function answering the question "Can this key be used to decrypt the binary file?"

## Format.  

Key format: DATA-HASH-TAIL.  

| Part | Description |
| :----: | :---- |
| Data | A part of the key encrypted with a password. Contains the key expiration date and application options. |
| Hash | Checksum of the key expiration date, password, options and environment parameters. |
| Tail | Initialization vector that used to decode the data. |

For example, KCATBZ14Y-VGDM2ZQ-ATSVYMI.

## Futures.  

- Generation of an activation key and its verification.
- Setting and restoration of information about user restrictions and permissions.  
- Using built in or specified encryption and hash algorithms.

## Key binding.  

Activation key is generated and verified using the following parameters:
- **expiration date** - limits the program's validity to the specified date. If value is ommited, it does not expire.  
- **password** - an optional parameter, assumes that the user must enter the correct password to run the program. If you pass null, then password is not used.    
- **options** - information that is restored when checking the key in its original form; may contain data such as the maximum number of launches, a key for decrypting a program block, restrictions and permisions to use any functions and other parameters necessary for the correct operation of the program. A value null for this parameter, when validated, will return an empty byte array.  
- **environment** - parameters for binding to the environment. These may include the name and version of the application, workstation ID, username, etc. If you do not specify environment parameters, then the key will not take any bounds.  

Thus, a range of tasks is solved:
- limiting the period of use of the program;
- limiting the distribution of the program to other computers;
- accounting of usernames and passwords;
- differentiation of user access rights to various program functions;
- storage in the key important information, without which application launch is impossible, for example you can add an cryptographyc token for encrypted assembly.

It is also possible to create a key without any limits.

## Usage.  
  
Just add *ActivationKey.cs* source file to your C# project and type the following directive to getting access **ActivationKey** somewhere you need:

```csharp
using System.Security.Cryptography;
```

### Example of generating a key.  

```csharp
string macAddress = NetworkInterface // Getting first MAC address
  NetworkInterface.
  GetAllNetworkInterfaces()
  .FirstOrDefault(nic => nic.OperationalStatus == OperationalStatus.Up 
    && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
  ?.GetPhysicalAddress()
  .ToString();
const string appName = "myAppName"; // The application name.

// Generating the key. All the parameters passed to the costructor can be omitted.
ActivationKey activationKey = new ActivationKey(
//expirationDate:
DateTime.Now.AddMonths(1),       // Expiration date 1 month later.
                                 // Pass DateTime.Max for unlimited use.
//password:
"password",                      // Password protection;
                                 // this parameter can be null.
//options:
"john",                          // Registered user name, for example.
                                 // Pass here numbers, flags, text or other
                                 // that you want to restore from the activation key.
//environment:
appName, macAddress              // Application name and MAC adress.
                                 // Pass here information about binding the key 
                                 // to a specific software and hardware environment. 
);
```
This code creates an activation key that looks like this:  
XO1UCW1FHEBVYZWLW1HA-RQUJ3EY-BBRNV2Q.

### Example of checking a key.  

```csharp
// Thus, a simple check of the key for validity is carried out.
bool checkKey = activationKey.Verify("password", appName, macAddress);
if (!checkKey)
{
  MessageBox.Show("Your copy is not activated! Please get a valid activation key.");
  Application.Exit();
}
```

### Example of checking a key using login and password.  

```csharp
// This way the options are restored as a byte array or null if the key is not valid. 
// If the key has no embeded options, an empty array will be returned.
Console.WriteLine("Login: ");
string login = Console.ReadLine();
Console.WriteLine("Password: ");
string password = Console.ReadLine();
byte[] bytes = activationKey.GetOptions(password, appName, macAddress);
if (bytes == null || Encoding.UTF8.GetString(bytes) != login)
{
  Console.Error.WriteLine("You are unregistered user.");
  Environment.Exit(1); // Exit process with error level 1.
}
```

### Example of using custom encrypt and hash algorithms.  

```csharp
ActivationKey activationKey = ActivationKey.Create<AesManaged, MD5CryptoServiceProvider>
  (DateTime.Now.AddMonths(1), "password", "john", "myAppName", "A1B2C3D4E5F6");

byte[] restoredOptions = activationKey.GetOptions<AesManaged, MD5CryptoServiceProvider>
  ("password", "myAppName", "A1B2C3D4E5F6");

bool valid = activationKey.Verify<AesManaged, MD5CryptoServiceProvider>
  ("password", "myAppName", "A1B2C3D4E5F6");
```

This code creates an activation key that looks like this:  
HWAPFVNL3XG5WPO3U3SHNWRBFDWZXPTSRK2BA5U4KU1QSBDTGWPQ-OEZHVY6VM4YGAW2CFZ31SDQ6CM-IO5Y4TIMIJFJBOOGEFUQI53T1M  

As you can see, using cryptographic aggregates like AES and MD5 creates keys that are too long. Such keys are not very convenient, but they provide more reliable cryptographic strength. 

## Details.

### How the key is generated.  

1. Creates an encryption engine using a password and stores the initialization vector in the **Tail** property.  
2. Next step, expiration date and options are encrypted and the encrypted data is saved into the **Data** property.
3. Finally, the hashing engine calculates a hash based on the expiration date, password, options and environment and puts it in the **Hash** property. 

### Initialization.  

The main initializer for a new instance of ActivationKey has the following parameters:  

| Parameter name | Description |
| :----: | :---- |
| expirationDate | The expiration date of the activation key. Since this date, any key validation check fails. |
| password | The password that the user must enter to successfully confirm their access right. It is recommended to use it for applications where login and password are supposed to be entered. This password is used to encrypt the key data. Pass null for default empty password using. |
| options | Application options to be embedded in the key. The data passed here is serialized into a byte array automatically and can be recovered as a byte array during key checking. Be aware that it is up to the custom code to deserialize the options back to the original objects. |
| environment | All data related to the binding of the key to a specific environment. These can include the title and version of the application, the name of the registered user, the hardware ID, and more, making the use of the key unique. This data turns into a hash and cannot be recovered in any view, only verified during key checking. |

<details><summary>View code...</summary>

```csharp
public ActivationKey(DateTime expirationDate, 
  object password, 
  object options = null,
  params object[] environment)
{
  if (password == null) password = new byte[0];
  byte[] iv = new byte[4]; // Initialization vector.
  byte[] key = Serialize(password); // encryption key
  InternalRng.GetBytes(iv); // Randomize bytes.
  using (_ARC4 arc4 = new _ARC4(key, iv))
  {
    expirationDate = expirationDate.Date;
    long expirationDateStamp = expirationDate.ToBinary();
    // Encrypting data part of the key.
    Data = arc4.Cipher(expirationDateStamp, options);
    _SMHasher mmh3 = new _SMHasher();
    // Calculating hash of the key.
    Hash = mmh3.GetBytes(expirationDateStamp, password, options, environment, iv);
    // Initialization vector increases the cryptographic strength.
    Tail = iv;
  }
}
```
</details>

You can also initialize the activation key from the existing data of the previously generated key.  

<details><summary>View code...</summary>

```csharp
// From presaved Data, Hash and Tail properties.
public ActivationKey(byte[] data, byte[] hash, byte[] tail);

// From a readable textual representation.
public ActivationKey(string activationKey);
```

</details>
  
### Verifying.  

Key verification is carried out using methodes **GetOptions** an **Verify**. 
- **GetOptions** checks the key and restores embeded data as byte array or null if key is not valid.  
- **Verify** just checks the key.  

The above methods used the same parameters as in the constructor, I will not describe them here.  

<details><summary>View code...</summary>

```csharp
public byte[] GetOptions(object password = null, params object[] environment)
{
  if (Data == null || Hash == null || Tail == null) return null;
  try
  {
    byte[] key = Serialize(password);
    using (_ARC4 arc4 = new _ARC4(key, Tail))
    {
      // Decrypting the data.
      byte[] data = arc4.Cipher(Data);
      int optionsLength = data.Length - 8;
      if (optionsLength < 0)
      {
        return null;
      }
      // Slicing the options from data.
      byte[] options;
      if (optionsLength > 0)
      {
        options = new byte[data.Length - 8];
        Buffer.BlockCopy(data, 8, options, 0, optionsLength);
      }
      else
      {
        options = new byte[0];
      }
      // Checking expiration date.
      long expirationDateStamp = BitConverter.ToInt64(data, 0);
      DateTime expirationDate = DateTime.FromBinary(expirationDateStamp);
      if (expirationDate < DateTime.Today)
      {
        return null;
      }
      // Checking the hash for verifying key.
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

public bool Verify(object password = null, params object[] environment)
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
```

</details>
                                          
### Text representation.  
  
Use the **ToString()** overriden method to get a string containing the key text, ready to be transfering to the end user.  
The additional **ToString(format)** method is useful for generating a description string in a custom format. Format can include substrings *"%D"* as data, *"%H"* as hash and *"%T"* as tail, which will be replaced by the corresponding parts of the key. You can also use string interpolation with identifiers D as data, H as hash and T as tail.

<details><summary>View code...</summary>
  
```csharp
string text = activationKey.ToString(); 
// returns KCATBZ14Y-VGDM2ZQ-ATSVYMI

text = activationKey.ToString("Key data is %D,\r\nkey hash is %H,\r\nkey tail is %T.");
//  - or -
text = $"Key data is {activationKey:D},\r\nkey hash is {activationKey:H},\r\nkey tail is {activationKey:T}."; 
// returns
// Key data is KCATBZ14Y, 
// key hash is VGDM2ZQ,
// key tail is ATSVYMI.
```

</details>  
  
### About conversion objects.  
  
The method **Serialize(objects)**  deserves a separate mention. In the beginning, I only used strings as key constructor parameters. Over time, I've come to the conclusion that supporting any types is a good idea, since converting them to a string and then representing the strings as bytes entails additional computational overhead and the length of the resulting key. Now the parameters used in the constructor and validation methods are of type **object**. This means that you can pass strings, numbers, bytes and other parameters that can be using as parameters for **Serialize** method to convert them to bytes array. This bytes is used to create the encrypted part of the key (**Data** property) or to calculate the hash (**Hash** property).  

<details><summary>View code...</summary>

```csharp
// You can improve it however you find it necessary for your own stuff.
static unsafe byte[] Serialize(params object[] objects)
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
                    case object o when obj.GetType().IsSerializable:
                        continue;
                    default:
                        int size = Marshal.SizeOf(obj);
                        byte[] bytes = new byte[size];
                        IntPtr handle = Marshal.AllocHGlobal(size);
                        try
                        {
                            Marshal.StructureToPtr(obj, handle, false);
                            Marshal.Copy(handle, bytes, 0, size);
                            writer.Write(bytes);
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(handle);
                        }
                        continue;
                }
            }
            catch (Exception e)
            {
              // This is where the debugger information will be helpful
            }
        }
        writer.Flush();
        byte[] result = memory.ToArray();
        return result;
    }
}
```

</details>
  
### Briefly about embeded classes.   

| Class | Description |  
| :---: | :-------- |  
| ARC4 | Port of cryptography provider designed by Ron Rivest © for encrypt/decrypt the data part. |  
| SMHasher | Port of Murmur Hash 3 designed by Austin Appleby © algorithm for calculating the hash. |  
| Base32 | Fork of Base-32 numeral system encoder designed by Denis Zinchenko © for converting the key to readeble text. |  

You can replace them with your own implementation of encryption, hash calculation and encoding of data.

[↑ Back to contents.](#contents)
