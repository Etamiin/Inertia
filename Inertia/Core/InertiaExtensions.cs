using System;
using System.Collections.Generic;
using System.Text;
using Inertia.Internal;

namespace Inertia
{
    /// <summary>
    /// Contains all the library extension
    /// </summary>
    public static class InertiaExtensions
    {
        #region General Extensions

        /// <summary>
        /// Shuffle the list
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="collection">List to shuffle</param>
        public static void Shuffle<T>(this IList<T> collection)
        {
            var iStart = 0;
            T valueSaved;

            while (iStart < collection.Count - 1)
            {
                int iRand = InertiaIO.m_rand.Next(iStart, collection.Count);
                valueSaved = collection[iStart];
                collection[iStart++] = collection[iRand];
                collection[iRand] = valueSaved;
            }
        }

        /// <summary>
        /// Get the SHA256 key representation of the specified string 
        /// </summary>
        /// <param name="text">Target string</param>
        /// <returns></returns>
        public static string GetSHA256(this string text)
        {
            return text.GetSHA256(Encoding.UTF8);
        }
        /// <summary>
        /// Get the SHA256 key representation of the specified string
        /// </summary>
        /// <param name="text">Target string</param>
        /// <param name="encoding">Target <see cref="Encoding"/> algorithm to use</param>
        /// <returns></returns>
        public static string GetSHA256(this string text, Encoding encoding)
        {
            var data = encoding.GetBytes(text);
            return InertiaIO.GetSHA256(data);
        }

        /// <summary>
        /// Get the current logger used (<see cref="BaseLogger.DefaultLogger"/>)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static BaseLogger GetLogger(this object obj)
        {
            return BaseLogger.DefaultLogger;
        }

        #endregion

        #region Storage Extensions

        internal static TypeCode ToTypeCode(this Type type)
        {
            if (type == typeof(bool))
                return TypeCode.Boolean;
            else if (type == typeof(char))
                return TypeCode.Char;
            else if (type == typeof(sbyte))
                return TypeCode.SByte;
            else if (type == typeof(byte) || type == typeof(byte[]))
                return TypeCode.Byte;
            else if (type == typeof(short))
                return TypeCode.Int16;
            else if (type == typeof(ushort))
                return TypeCode.UInt16;
            else if (type == typeof(int))
                return TypeCode.Int32;
            else if (type == typeof(uint))
                return TypeCode.UInt32;
            else if (type == typeof(long))
                return TypeCode.Int64;
            else if (type == typeof(ulong))
                return TypeCode.UInt64;
            else if (type == typeof(float))
                return TypeCode.Single;
            else if (type == typeof(double))
                return TypeCode.Double;
            else if (type == typeof(decimal))
                return TypeCode.Decimal;
            else if (type == typeof(string))
                return TypeCode.String;
            else
                return TypeCode.Object;
        }
        internal static Type ToType(this TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Boolean:
                    return typeof(bool);
                case TypeCode.Char:
                    return typeof(char);
                case TypeCode.SByte:
                    return typeof(sbyte);
                case TypeCode.Byte:
                    return typeof(byte);
                case TypeCode.Int16:
                    return typeof(short);
                case TypeCode.UInt16:
                    return typeof(ushort);
                case TypeCode.Int32:
                    return typeof(int);
                case TypeCode.UInt32:
                    return typeof(uint);
                case TypeCode.Int64:
                    return typeof(long);
                case TypeCode.UInt64:
                    return typeof(uint);
                case TypeCode.Single:
                    return typeof(float);
                case TypeCode.Decimal:
                    return typeof(decimal);
                case TypeCode.String:
                    return typeof(string);
            }

            return typeof(object);
        }

        #endregion

        #region Compression extensions

        /// <summary>
        /// Compress the specified byte array and return the compressed one
        /// </summary>
        /// <param name="data">Target byte array to compress</param>
        /// <param name="compressed">Return true if the returned data is lower in length than the non-compressed data</param>
        /// <returns>Compressed byte array</returns>
        public static byte[] Compress(this byte[] data, out bool compressed)
        {
            return InertiaIO.Compress(data, out compressed);
        }
        /// <summary>
        /// Decompress the specified byte array and return the decompressed one
        /// </summary>
        /// <param name="compressedData">Target byte array to decompress</param>
        /// <returns>Decompressed byte array</returns>
        public static byte[] Decompress(this byte[] compressedData)
        {
            return InertiaIO.Decompress(compressedData);
        }

        /// <summary>
        /// Encrypt the target byte array with the specified string key
        /// </summary>
        /// <param name="data">Target byte array to encrypt</param>
        /// <param name="key">Target string key for encryption</param>
        /// <returns>Encrypted byte array</returns>
        public static byte[] EncryptWithString(this byte[] data, string key)
        {
            return InertiaIO.EncryptWithString(data, key);
        }
        /// <summary>
        /// Encrypt the target byte array with the specified string key
        /// </summary>
        /// <param name="encryptedData">Target byte array to encrypt</param>
        /// <param name="key">Target string key for encryption</param>
        /// <returns>Decrypted byte array</returns>
        public static byte[] DecryptWithString(this byte[] encryptedData, string key)
        {
            return InertiaIO.DecryptWithString(encryptedData, key);
        }

        #endregion
    }
}
