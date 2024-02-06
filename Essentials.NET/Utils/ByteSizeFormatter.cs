namespace Essentials.NET;

public static class ByteSizeFormatter
{
    private static readonly string[] sizes = { "kB", "MB", "GB", "TB", "PB" };

    /// <summary>
    /// Formats a size in bytes as a readable string according to the International System of Units (SI).
    /// </summary>
    /// <param name="size">The size to format in bytes</param>
    /// <param name="decimals">The number of decimals used if the size is greater than or equal to 1000 bytes</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information</param>
    /// <returns>A formatted string</returns>
    public static string Format(int size, int decimals = 2, IFormatProvider? formatProvider = null)
    {
        return Format((ulong)size, decimals, formatProvider);
    }

    /// <summary>
    /// Formats a size in bytes as a readable string according to the International System of Units (SI).
    /// </summary>
    /// <param name="size">The size to format in bytes</param>
    /// <param name="decimals">The number of decimals used if the size is greater than or equal to 1000 bytes</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information</param>
    /// <returns>A formatted string</returns>
    public static string Format(uint size, int decimals = 2, IFormatProvider? formatProvider = null)
    {
        return Format((ulong)size, decimals, formatProvider);
    }

    /// <summary>
    /// Formats a size in bytes as a readable string according to the International System of Units (SI).
    /// </summary>
    /// <param name="size">The size to format in bytes</param>
    /// <param name="decimals">The number of decimals used if the size is greater than or equal to 1000 bytes</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information</param>
    /// <returns>A formatted string</returns>
    public static string Format(long size, int decimals = 2, IFormatProvider? formatProvider = null)
    {
        return Format((ulong)size, decimals, formatProvider);
    }

    /// <summary>
    /// Formats a size in bytes as a readable string according to the International System of Units (SI).
    /// </summary>
    /// <param name="size">The size to format in bytes</param>
    /// <param name="decimals">The number of decimals used if the size is greater than or equal to 1000 bytes</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information</param>
    /// <returns>A formatted string</returns>
    public static string Format(ulong size, int decimals = 2, IFormatProvider? formatProvider = null)
    {
        int order = 0;
        while (size >= 1000 * 1000 && order < sizes.Length - 1)
        {
            order++;
            size /= 1000;
        }
        if (size >= 1000)
        {
            string numberFormat = "0." + new string(Enumerable.Repeat('0', decimals).ToArray());
            return (size / 1000d).ToString(numberFormat, formatProvider) + " " + sizes[order];
        }
        return size + " " + (size > 1 ? "Bytes" : "Byte");
    }

}