# Activation Key.
Represents the activation key used to protect your C# application. The key can be stored as a human readable text for easy transfering to the end user. 
Contains methods for generating the cryptography key based on the specified hardware and software environment. An additional feature is the ability to embed any information into the key. This information can be recovered as a byte array during key verifying.  

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

It is also possible to create a key without any restrictions.

## Details.
### Initialization.
Main initializers of a new instance of ActivationKey look like this:
```csharp
public ActivationKey(DateTime expirationDate, byte[] password, object options = null, params object[] environment);
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
Key verification is carried out using methodes GetOptions an Verify. 

| Parameter name | Description |
| :----: | :---- |
| GetOptions | Checks the key and restores embeded data as byte array or null if key is not valid. |
| Verify | Just checks the key. |

### Text representation.
Use the **ToString()** overriden method to get a string containing the key text, ready to be transfering to the end user.  
The additional **ToString(format)** method is useful for generating a description string in a custom format. Format can include substrings *{data}*, *{hash}* and *{tail}* as substitution parameters, which will be replaced by the corresponding parts of the key.  

### How the key is generated.
1. Creates an encryption engine using a password and stores the initialization vector in the **Tail** property. Then  
2. The expiration date and options are encrypted and the encrypted data is saved into the **Data** property.
3. The hashing engine calculates a hash based on the expiration date, password, options and environment and puts it in the **Hash** property. 

## Usage.
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
