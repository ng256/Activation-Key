# Activation Key.
Represents the activation key used to protect your C# application. It is also called a license key, product key, product activation, software key and even a serial number. It is a specific software-based key for a computer program. It certifies that the copy of the program is original.  
The key can be stored as a human readable text for easy transfering to the end user.  
Contains methods for generating the cryptography key based on the specified hardware and software binding. An additional feature is the ability to embed any information directly into the key. This information can be recovered as a byte array during key verifying.  

## Format. 
Key format: DATA-HASH-TAIL
| Part | Description |
|:----:|:----|
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

## Details.

### How the key is generated.
1. Creates an encryption engine using a password and stores the initialization vector in the **Tail** property.  
2. Next step, expiration date and options are encrypted and the encrypted data is saved into the **Data** property.
3. Finally, the hashing engine calculates a hash based on the expiration date, password, options and environment and puts it in the **Hash** property. 

### Initialization.
Main initializers of a new instance of ActivationKey look like this:
```csharp
public ActivationKey(DateTime expirationDate, object password, object options = null, params object[] environment)
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
| Parameter name | Description |
| :----: | :---- |
| expirationDate | The expiration date of the activation key. Since this date, any key validation check fails. |
| password | The password that the user must enter to successfully confirm their access right. It is recommended to use it for applications where login and password are supposed to be entered. This password is used to encrypt the key data. Pass null for default empty password using. |
| options | Application options to be embedded in the key. The data passed here is serialized into a byte array automatically and can be recovered as a byte array during key checking. Be aware that it is up to the custom code to deserialize the options back to the original objects. |
| environment | All data related to the binding of the key to a specific environment. These can include the title and version of the application, the name of the registered user, the hardware ID, and more, making the use of the key unique. This data turns into a hash and cannot be recovered in any view, only verified during key checking. |

You can also initialize the activation key from the existing data of the previously generated key. 

```csharp
// From presaved Data, Hash and Tail properties.
public ActivationKey(byte[] data, byte[] hash, byte[] tail);

// From a readable textual representation.
public ActivationKey(string activationKey);
```

### Verifying.
Key verification is carried out using methodes **GetOptions** an **Verify**. 
- **GetOptions** checks the key and restores embeded data as byte array or null if key is not valid.  
- **Verify** just checks the key.  

The above methods used the same parameters as in the constructor, I will not describe them here.  
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

### Text representation.
Use the **ToString()** overriden method to get a string containing the key text, ready to be transfering to the end user.  
The additional **ToString(format)** method is useful for generating a description string in a custom format. Format can include substrings *{data}*, *{hash}* and *{tail}* as substitution parameters, which will be replaced by the corresponding parts of the key.  

### About conversion objects.
The method **Serialize(objects)**  deserves a separate mention. In the beginning, I only used strings as key constructor parameters. Over time, I've come to the conclusion that supporting any types is a good idea, since converting them to a string and then representing the strings as bytes entails additional computational overhead and the length of the resulting key. Now the parameters used in the constructor and validation methods are of type **object**. This means that you can pass strings, numbers, bytes and other parameters that can be using as parameters for **Serialize** method to convert them to bytes array. This bytes is used to create the encrypted part of the key (**Data** property) or to calculate the hash (**Hash** property).  

```csharp
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
        // Using secure string is best solution to manage password.
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
        // Generic types.
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
        // Other types.
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
            // Place debugging tools here.
          }
          continue;
      }
    }
    writer.Flush();
    byte[] bytes = memory.ToArray();
    return bytes;
  }
}
```

### Briefly about built-in classes. 
| Class | Description |  
| :---: | :-------- |  
| ARC4 | Port of cryptography provider designed by Ron Rivest © for encrypt/decrypt the data part. |  
| SMHasher | Port of Murmur Hash 3 designed by Austin Appleby © algorithm for calculating the hash. |  
| Base32 | Fork of Base-32 numeral system encoder designed by Denis Zinchenko © for converting the key to readeble text. |  

## Usage.
Just add *ActivationKey.cs* source file to your C# project and type the following directive to getting access **ActivationKey** somewhere you need:
```csharp
using System.Security.Cryptography;
```

### Example of generating a key.
```csharp
// Generating the key. All the parameters passed to the costructor can be omitted.
ActivationKey activationKey = new ActivationKey(
expirationDate:
DateTime.Now.AddMonths(1),       // Expiration date 1 month later.
                                 // Pass DateTime.Max for unlimited use.
password:
"password",                      // Password protection;
                                 // this parameter can be null.
options:
"permitSave",                    // Option that allows save data, for example. 
                                 // Pass here numbers, flags, text
                                 // that you want to restore from the activation key.
environment:
"myAppName", "01:02:03:04:05:06" // App name and MAC adress.
                                 // Pass here information about binding the key 
                                 // to a specific software and hardware environment. 
);
```
This code creates an activation key that looks like this:  
A5IE5SYJQIZEM4CTIHRWX2FDV6LO5-KAGJRCQ-KRW3MSA. 

### Example of checking a key.
```csharp
// This way the options are restored as a byte array or null if the key is not valid. 
// If the key has no embeded options, an empty array will be returned.
byte[] restoredOptions = activationKey.GetOptions("password", "myAppName", "01:02:03:04:05:06");

// Thus, a simple check of the key for validity is carried out.
bool checkKey = activationKey.Verify("password", "myAppName", "01:02:03:04:05:06");
```

### Example of using custom encrypt and hash algorithms.
```csharp
ActivationKey activationKey = ActivationKey.Create<AesManaged, MD5CryptoServiceProvider>
  (DateTime.Now.AddMonths(1), "password", "permitSave", "myAppName", "01:02:03:04:05:06");

byte[] restoredOptions = activationKey.GetOptions<AesManaged, MD5CryptoServiceProvider>
  ("password", "myAppName", "01:02:03:04:05:06");

bool valid = activationKey.Verify<AesManaged, MD5CryptoServiceProvider>
  ("password", "myAppName", "01:02:03:04:05:06");
```
This code creates an activation key that looks like this:  
HWAPFVNL3XG5WPO3U3SHNWRBFDWZXPTSRK2BA5U4KU1QSBDTGWPQ-OEZHVY6VM4YGAW2CFZ31SDQ6CM-IO5Y4TIMIJFJBOOGEFUQI53T1M  

As you can see, using cryptographic aggregates like AES and MD5 creates keys that are too long. Such keys are not very convenient, but they provide more reliable cryptographic strength. 
