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
    /// Checks if the difference of the given values is smaller than the specified delta.
    public static bool ApproximateEquals(double valueA, double valueB, double delta = 0.001)
    {
        return Diff(valueA, valueB) < delta;
    }

    /// <summary>
    /// Checks if the difference of the given values is smaller than the specified delta.
    public static bool ApproximateEquals(float valueA, float valueB, float delta = 0.001f)
    {
        return Diff(valueA, valueB) < delta;
    }
}