/***************************************************************

•   File: InternalTools.cs

•   Description
.
    The code  is  a set  of methods and for working with various 
    data  types.

    Methods of the InternalTools perform the following functions:

    Working with resources: The code allows you to get resources
    from assemblies and Mscorlib.

    Random  Data  Generation:   The   code provides  methods for
    generating  random  bytes and  structures.

    Copying and comparing arrays: The code includes  methods for
    copying,   comparing,  and    checking  arrays  for  invalid
    characters.

    String  checking: The   code provides methods  for comparing
    strings, checking them for emptiness and invalid characters.

    Working with the registry: the code allows you to get values
    from  the  registry.

    Serializing  and Deserializing Objects: The code  provides a
    method for serializing objects into a byte array.

***************************************************************/

using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace System
{
    internal static class InternalTools
    {
        #region Static members
        private static Hashtable _resources;
        private static ResourceSet _mscorlib; // Mscorlib resources.
        private static RNGCryptoServiceProvider _rng = null;
        internal static RNGCryptoServiceProvider StaticRandomNumberGenerator => _rng ?? (_rng = new RNGCryptoServiceProvider());

        internal static Hashtable Resources => _resources ?? (_resources = new Hashtable());
        internal static ResourceSet MSCorLib => _mscorlib ?? (_mscorlib = GetResources()); // Mscorlib resources.
        #endregion

        // Returns a set of resources from the specified assembly.
        private static ResourceSet GetResources(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly),
                    GetResourceString("ArgumentNull_Assembly"));
            }

            Hashtable resources = Resources;
            AssemblyName assemblyName = assembly.GetName();
            if (resources.ContainsKey(assemblyName.Name))
            {
                return (ResourceSet)resources[assemblyName.Name];
            }

            ResourceManager resManager = new ResourceManager(assemblyName.Name, assembly);
            ResourceSet resourceSet = resManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            resources.Add(assemblyName, resourceSet);
            return resourceSet;
        }

        // Returns a set of resources from the assembly that implements the specified type.
        private static ResourceSet GetResources(Type type = null)
        {
            if (type == null)
            {
                type = typeof(object);
            }

            return GetResources(type.Assembly);
        }

        // Gets mscorlib internal error message.
        internal static string GetResourceString(string resourceName)
        {
            return MSCorLib.GetString(resourceName);
        }

        // Gets parametrized error message for assembly contains the specified type.
        internal static string GetResourceString<T>(string resourceName)
        {
            return GetResources(typeof(T)).GetString(resourceName);
        }

        // Gets parametrized mscorlib internal error message.
        internal static string GetResourceString(string resourceName, params object[] args)
        {
            return string.Format(GetResourceString(resourceName), args);
        }

        // Gets parametrized error message for assembly contains the specified type.
        internal static string GetResourceString<T>(string resourceName, params object[] args)
        {
            return string.Format(GetResourceString<T>(resourceName), args);
        }

        // Generates an array of random bytes of a given size.
        internal static byte[] GenerateRandom(int size)
        {
            byte[] data = new byte[size];
            StaticRandomNumberGenerator.GetBytes(data);
            return data;
        }

        // Generates a random struct.
        internal static unsafe T GenerateRandom<T>() where T: unmanaged
        {
            byte[] bytes = GenerateRandom(sizeof(T));
            fixed (byte* buffer = bytes)
            {
                return *(T*)buffer;
            }
        }

        // Converts a byte array to a value of type T
        internal static unsafe T ConvertTo<T>(byte[] bytes, int offset = 0) where T : unmanaged
        {
            fixed (byte* buffer = bytes)
            {
                return *(T*)(buffer + offset);
            }
        }

        // Converts a value of type T to a byte array.
        internal static unsafe byte[] GetBytes<T>(T value) where T : unmanaged
        {
            byte[] bytes = new byte[sizeof(T)];
            fixed (byte* buffer = bytes)
            {
                *(T*) buffer = value;
                return bytes;
            }
        }

        // Returns the maximum possible length to retrieve elements from an array, starting at a specified position.
        internal static int GetMaxCount<T>(this T[] array, int startIndex, int count)
        {
            return Math.Min(count, array.Length - startIndex);
        }

        // Returns the maximum possible length to retrieve elements from an array, starting at a specified position.
        internal static int GetMaxCount<T>(this T[] array, int startIndex)
        {
            return array.Length - startIndex;
        }

        // Checks whether array is empty or null.
        internal static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        // Create copy of array.
        internal static T[] ArrayClone<T>(this T[] array)
        {
            T[] newArray = new T[array.Length];
            array.CopyTo( newArray, 0 );
            return newArray;
        }

        // Creates a copy of part of the array starting at the specified index.
        internal static T[] ArrayClone<T>(this T[] array, int index)
        {
            T[] newArray = new T[array.Length - index];
            for (int i = index, j = 0; i < array.Length; i++)
            {
                newArray[j++] = array[i];
            }

            return newArray;
        }

        // Creates a copy of the specified number of array elements, starting at the specified index.
        internal static T[] ArrayClone<T>(this T[] array, int index, int count)
        {
            T[] newArray = new T[count];
            for (int i = index, j = 0; i <= index + count; i++)
            {
                newArray[j++] = array[i];
            }

            return newArray;
        }

        // Allows you to compare strings using the specified StringComparer.
        public static bool Equals(this string s, string value, StringComparer comparer)
        {
            return comparer.Equals(s, value);
        }

        // Checks whether the fileName string contains invalid characters for the path.
        public static bool IsInvalidPath(string fileName)
        {
            return fileName.Any(c => Path.GetInvalidPathChars().Contains(c));
        }

        // Checks whether the string is null, empty or contains invalid characters for the path.
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrEmpty(s) || s.All(char.IsWhiteSpace);
        }

        // Checks whether the string is null or empty.
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        // Checks whether a character is printable.
        internal static bool IsPrintableCharacter(this char c)
        {
            return char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c);
        }

        // Checks whether all characters are printable.
        internal static bool IsPrintableCharacters(this IEnumerable<char> chars)
        {
            return chars.All(c => c.IsPrintableCharacter());
        }

        internal static void Clear<T>(this T[] array)
        {
            if (array != null)
            {
                Array.Clear(array, 0, array.Length);
            }
        }

        // Compares two arrays.
        internal static bool ArrayEquals<T>(this T[] array, T[] otherArray) where T : struct
        {
            if ((array == null) ^ (otherArray == null))
            {
                return false;
            }

            if (array == otherArray)
            {
                return true;
            }

            if (array.Length != otherArray.Length)
            {
                return false;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].Equals(otherArray[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Compares two arrays using the specified comparer.
        internal static bool ArrayEquals<T>(this T[] array, T[] otherArray, IEqualityComparer<T> comparer) where T : struct
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            if ((array == null) ^ (otherArray == null))
            {
                return false;
            }

            if (array == otherArray)
            {
                return true;
            }

            if (array.Length != otherArray.Length)
            {
                return false;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (!comparer.Equals(array[i], otherArray[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Checks whether array contains the same values.
        internal static bool ContainsDuplicates<T>(this T[] array) where T : struct
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (array[i].Equals(array[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Checks whether array contains the same values using the specified comparer.
        internal static bool ContainsDuplicates<T>(this T[] array, IEqualityComparer<T> comparer) where T : struct
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            for (int i = 0; i < array.Length; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (comparer.Equals(array[i], array[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Gets a value from the registry indicating RegistryHive, section and parameter name
        public static object GetRegistryValue(RegistryHive hive, string key, string parameter, object defaultValue)
        {
            using (RegistryKey regKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default).OpenSubKey(key))
            {
                return regKey?.GetValue(parameter, defaultValue);
            }
        }

        public static object GetRegistryValue(string key, string parameter, object defaultValue)
        {
            return Registry.GetValue(key, parameter, defaultValue);
        }

        // Converts objects to a byte array. You can improve it however you find it necessary for your own stuff.
        [SecurityCritical]
        internal static unsafe byte[] Serialize(params object[] objects)
        {
            if (objects == null)
            {
                return new byte[0];
            }

            using (MemoryStream memory = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memory))
            {
                for (int j = 0; j < objects.Length; j++)
                {
                    object obj = objects[j];
                    if (obj == null)
                    {
                        continue;
                    }

                    try
                    {
                        switch (obj)
                        {
                            case null:
                                continue;
                            case SecureString secureString:
                                if (secureString == null || secureString.Length == 0)
                                {
                                    continue;
                                }

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
                                writer.Write(GetBytes(date));
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
                            case ValueType @struct:
                                int size = Marshal.SizeOf(@struct);
                                byte[] bytes = new byte[size];
                                IntPtr handle = Marshal.AllocHGlobal(size);
                                try
                                {
                                    Marshal.StructureToPtr(@struct, handle, false);
                                    Marshal.Copy(handle, bytes, 0, size);
                                    writer.Write(bytes);
                                }
                                finally
                                {
                                    Marshal.FreeHGlobal(handle);
                                }
                                continue;
                            default:
                                IFormatter formatter = new BinaryFormatter();
                                formatter.Serialize(memory, obj);
                                continue;
                        }
                    }
                    catch (Exception e)
                    {
#if DEBUG               // This is where the debugger information will be helpful
                        if (Debug.Listeners.Count > 0)
                        {
                            Debug.WriteLine(DateTime.Now);
                            Debug.WriteLine(GetResourceString("Arg_SerializationException"));
                            Debug.WriteLine(GetResourceString("Arg_ParamName_Name", $"{nameof(objects)}[{j}]"));
                            Debug.WriteLine(obj, "Object");
                            Debug.WriteLine(e, "Exception");
                        }
#endif
                    }
                }
                writer.Flush();
                byte[] result = memory.ToArray();
                return result;
            }
        }
    }
}
