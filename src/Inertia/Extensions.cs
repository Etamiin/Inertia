using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Inertia;

public static class Extensions
{
    private readonly static Random Randomizer = new Random();

    public static void Shuffle<T>(this IList<T> collection)
    {
        var iStart = 0;
        T savedValue;

        while (iStart < collection.Count - 1)
        {
            int iRand = Randomizer.Next(iStart, collection.Count);
            savedValue = collection[iStart];
            collection[iStart++] = collection[iRand];
            collection[iRand] = savedValue;
        }
    }

    public static string GetSHA256(this string text)
    {
        return text.GetSHA256(Encoding.UTF8);
    }
    public static string GetSHA256(this string text, Encoding encoding)
    {
        return IOHelper.GetSHA256(encoding.GetBytes(text));
    }

    public static byte[] GzipCompress(this byte[] data, out bool hasBetterSize)
    {
        return IOHelper.GzipCompress(data, out hasBetterSize);
    }
    public static byte[] GzipDecompress(this byte[] compressedData)
    {
        return IOHelper.GzipDecompress(compressedData);
    }

    /// <summary>
    /// Create a byte flag containing specified boolean values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    /// <exception cref="BoolFlagTooLargeException"></exception>
    public static byte ToByte(this bool[] values)
    {
        if (values.Length > 8)
        {
            throw new BoolFlagTooLargeException();
        }

        byte result = 0;
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i]) result |= (byte)(1 << (7 - i));
        }

        return result;
    }
    /// <summary>
    /// Read boolean values from a byte flag
    /// </summary>
    /// <param name="value"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool[] ToBits(this byte value, int length)
    {
        var result = new bool[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = (value & (1 << i)) != 0;
        }

        Array.Reverse(result);
        return result;
    }
}