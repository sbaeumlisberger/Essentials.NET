namespace Essentials.NET;

public class MathUtils
{

    /// <summary>
    /// Gets the absolute difference between two longs.
    /// </summary>
    public static long Diff(long value1, long value2)
    {
        return Math.Abs(value1 - value2);
    }

    /// <summary>
    /// Gets the absolute difference between two int.
    /// </summary>
    public static int Diff(int value1, int value2)
    {
        return Math.Abs(value1 - value2);
    }

    /// <summary>
    /// Gets the absolute difference between two doubles.
    /// </summary>
    public static double Diff(double value1, double value2)
    {
        return Math.Abs(value1 - value2);
    }

    /// <summary>
    /// Gets the absolute difference between two floats.
    /// </summary>
    public static float Diff(float value1, float value2)
    {
        return Math.Abs(value1 - value2);
    }

    /// <summary>
    /// Clips the given value to the specified minimum and maximum.
    /// </summary>
    public static long Clip(long value, long min, long max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    /// <summary>
    /// Clips the given value to the specified minimum and maximum.
    /// </summary>
    public static int Clip(int value, int min, int max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    /// <summary>
    /// Clips the given value to the specified minimum and maximum.
    /// </summary>
    public static double Clip(double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    /// <summary>
    /// Clips the given value to the specified minimum and maximum.
    /// </summary>
    public static float Clip(float value, float min, float max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

}