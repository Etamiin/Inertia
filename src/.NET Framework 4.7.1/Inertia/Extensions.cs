using System.Collections.Generic;
using System.Text;
using System.IO;
using Inertia;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Shuffle the specified <see cref="IList{T}"/> object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    public static void Shuffle<T>(this IList<T> collection)
    {
        var iStart = 0;
        T savedValue;

        while (iStart < collection.Count - 1)
        {
            int iRand = IOHelper.Randomizer.Next(iStart, collection.Count);
            savedValue = collection[iStart];
            collection[iStart++] = collection[iRand];
            collection[iRand] = savedValue;
        }
    }

    /// <summary>
    /// Returns the SHA256 representation of the specified text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string GetSHA256(this string text)
    {
        return text.GetSHA256(Encoding.UTF8);
    }
    /// <summary>
    /// Returns the SHA256 representation of the specified text.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string GetSHA256(this string text, Encoding encoding)
    {
        return IOHelper.GetSHA256(encoding.GetBytes(text));
    }

    /// <summary>
    /// Compress and return the specified data.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="hasBetterSize">Returns true if the returned data is lower in length than the non-compressed data</param>
    /// <returns></returns>
    public static byte[] GzipCompress(this byte[] data, out bool hasBetterSize)
    {
        return IOHelper.GzipCompress(ref data, out hasBetterSize);
    }
    /// <summary>
    /// Decompress and return the specified data.
    /// </summary>
    /// <param name="compressedData"></param>
    /// <returns></returns>
    public static byte[] GzipDecompress(this byte[] compressedData)
    {
        return IOHelper.GzipDecompress(ref compressedData);
    }

    /// <summary>
    /// Encrypts the specified data with the specified key.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static byte[] EncryptWithString(this byte[] data, string key)
    {
        return IOHelper.EncryptWithString(data, key);
    }
    /// <summary>
    /// Decrypts the specified data with the specified key.
    /// </summary>
    /// <param name="encryptedData"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static byte[] DecryptWithString(this byte[] encryptedData, string key)
    {
        return IOHelper.DecryptWithString(encryptedData, key);
    }

    /// <summary>
    /// Create a byte flag containing specified boolean values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    /// <exception cref="BoolFlagTooLargeException"></exception>
    public static byte CreateFlag(this bool[] values)
    {
        if (values.Length > 8)
        {
            throw new BoolFlagTooLargeException();
        }

        var flag = (byte)0;
        for (var i = 0; i < values.Length; i++)
        {
            flag = values[i] ? (byte)(flag | (1 << i)) : (byte)(flag & 255 - (1 << i));
        }

        return flag;
    }
    /// <summary>
    /// Read boolean values from a byte flag
    /// </summary>
    /// <param name="flag"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool[] GetBits(this byte flag, int length)
    {
        var flags = new bool[length];
        for (var i = 0; i < length; i++)
        {
            flags[i] = (flag & (byte)(1 << i)) != 0;
        }

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
        {
            folderPath += @"\";
        }

        return folderPath;
    }
}