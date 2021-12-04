# Activation Key.
Represents the activation key used to protect your C# application. The key can be stored as a human readable text for easy transfering to the end user. 
Contains methods for generating the key based on the specified hardware and software environment.

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

## Examples.
### Example of generating a key.
```csharp
ActivationKey activationKey = new ActivationKey(
DateTime.Now.AddMonths(1),       // Expiration date 1 month later.

"password",                      // Password protection;
                                 // this parameter can be null.
                                 
"permitSave",                    // Option that allows save data, for example. 
                                 // Pass here numbers, flags, text
                                 // that you want to restore from the activation key.
                                 
"myAppName", "01:02:03:04:05:06" // App name and MAC adress.
                                 // Pass here information about binding the key 
                                 // to a specific software and hardware environment. 
);
```
This code creates an activation key that looks like this:  
A5IE5SYJQIZEM4CTIHRWX2FDV6LO5-KAGJRCQ-KRW3MSA. 

### Example of checking a key.
```csharp
byte[] restoredOptions = activationKey.GetOptions("password", "myAppName", "01:02:03:04:05:06");
bool checkKey = activationKey.Verify("password", "myAppName", "01:02:03:04:05:06");
```

### Example of using custom encrypt and hash algorithms.
```csharp
ActivationKey activationKey = ActivationKey.Create<AesManaged, MD5CryptoServiceProvider>(DateTime.Now.AddMonths(1), "password", "permitSave", "myAppName", "01:02:03:04:05:06");
byte[] restoredOptions = activationKey.GetOptions<AesManaged, MD5CryptoServiceProvider>("password", "myAppName", "01:02:03:04:05:06");
bool valid = activationKey.Verify<AesManaged, MD5CryptoServiceProvider>("password", "myAppName", "01:02:03:04:05:06");
```
This code creates an activation key that looks like this:  
HWAPFVNL3XG5WPO3U3SHNWRBFDWZXPTSRK2BA5U4KU1QSBDTGWPQ-OEZHVY6VM4YGAW2CFZ31SDQ6CM-IO5Y4TIMIJFJBOOGEFUQI53T1M  

As you can see, using cryptographic aggregates like AES and MD5 creates keys that are too long. Such keys are not very convenient, but they provide more reliable cryptographic strength. 
