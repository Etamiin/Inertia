using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Inertia;

/// <summary>
/// 
/// </summary>
public static class BasicExtensionsClass
{
    /// <summary>
    /// Shuffle the specified <see cref="IList{T}"/> object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"><see cref="IList{T}"/> to shuffle</param>
    public static void Shuffle<T>(this IList<T> collection)
    {
        var iStart = 0;
        T valueSaved;

        while (iStart < collection.Count - 1)
        {
            int iRand = InertiaIO.IoRandomizer.Next(iStart, collection.Count);
            valueSaved = collection[iStart];
            collection[iStart++] = collection[iRand];
            collection[iRand] = valueSaved;
        }
    }

    /// <summary>
    /// Get the SHA256 representation of the specified <see cref="string"/> value
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string GetSHA256(this string text)
    {
        return text.GetSHA256(Encoding.UTF8);
    }
    /// <summary>
    /// Get the SHA256 representation of the specified <see cref="string"/> value
    /// </summary>
    /// <param name="text"></param>
    /// <param name="encoding"><see cref="Encoding"/> to use</param>
    /// <returns></returns>
    public static string GetSHA256(this string text, Encoding encoding)
    {
        var data = encoding.GetBytes(text);
        return InertiaIO.GetSHA256(data);
    }

    /// <summary>
    /// Compress the specified data and returns the compressed one
    /// </summary>
    /// <param name="data">Target data to compress</param>
    /// <param name="isOptimized">Return true if the returned data is lower in length than the non-compressed data</param>
    /// <returns></returns>
    public static byte[] GzipCompress(this byte[] data, out bool isOptimized)
    {
        return InertiaIO.GzipCompress(data, out isOptimized);
    }
    /// <summary>
    /// Decompress the specified data and returns the decompressed one
    /// </summary>
    /// <param name="compressedData"></param>
    /// <returns></returns>
    public static byte[] GzipDecompress(this byte[] compressedData)
    {
        return InertiaIO.GzipDecompress(compressedData);
    }

    /// <summary>
    /// Encrypt the target data with the specified string key
    /// </summary>
    /// <param name="data">Target data to encrypt</param>
    /// <param name="key">Target string key for encryption</param>
    /// <returns>Encrypted byte array</returns>
    public static byte[] EncryptWithString(this byte[] data, string key)
    {
        return InertiaIO.EncryptWithString(data, key);
    }
    /// <summary>
    /// Decrypt the target data with the specified string key
    /// </summary>
    /// <param name="encryptedData">Target data to decrypt</param>
    /// <param name="key">Target string key for encryption</param>
    /// <returns>Decrypted byte array</returns>
    public static byte[] DecryptWithString(this byte[] encryptedData, string key)
    {
        return InertiaIO.DecryptWithString(encryptedData, key);
    }

    /// <summary>
    /// Get the <see cref="BaseLogger"/> instance
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static BaseLogger GetLogger(this object obj)
    {
        return BaseLogger.Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static byte CreateFlag(this bool[] values)
    {
        if (values.Length > 8)
            throw new BoolFlagTooLargeException();

        var flag = (byte)0;
        for (var i = 0; i < values.Length; i++)
            flag = values[i] ? (byte)(flag | (1 << i)) : (byte)(flag & 255 - (1 << i));

        return flag;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="flag"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool[] GetBits(this byte flag, int length)
    {
        var flags = new bool[length];
        for (var i = 0; i < length; i++)
            flags[i] = (flag & (byte)(1 << i)) != 0;

        return flags;
    }

    /* INTERNAL EXTENSIONS */
    internal static string ConventionFolderPath(this string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            folderPath = Directory.GetCurrentDirectory();
        }
        else if (!File.GetAttributes(folderPath).HasFlag(FileAttributes.Directory))
        {
            var info = new DirectoryInfo(folderPath);
            folderPath = info.FullName;
        }

        if (!folderPath.EndsWith(@"\"))
            folderPath += @"\";

        return folderPath;
    }
}