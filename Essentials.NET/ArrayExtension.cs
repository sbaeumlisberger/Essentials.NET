namespace Essentials.NET;


public static class ArrayExtension
{

    public static void Resize<T>(this T[] array, int newSize)
    {
        Array.Resize(ref array, newSize);
    }

}