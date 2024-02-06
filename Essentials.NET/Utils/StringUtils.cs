namespace Essentials.NET;

public static class StringUtils
{
    public static string ReplaceEnd(this string source, string end, string replacment)
    {
        if (source.EndsWith(end))
        {
            return string.Concat(source.AsSpan(0, source.Length - end.Length), replacment);
        }
        return source;
    }

    public static string RemoveEnd(this string source, string end)
    {
        if (source.EndsWith(end))
        {
            return source.Substring(0, source.Length - end.Length);
        }
        return source;
    }

    public static string ReplaceStart(this string source, string start, string replacment)
    {
        if (source.StartsWith(start))
        {
            return string.Concat(replacment, source.AsSpan(start.Length, source.Length - start.Length));
        }
        return source;
    }

    public static string RemoveStart(this string source, string start)
    {
        if (source.StartsWith(start))
        {
            return source.Substring(start.Length, source.Length - start.Length);
        }
        return source;
    }

    public static string JoinNonEmpty(string separator, params object?[] values)
    {
        return string.Join(separator, values.Where(x => !string.IsNullOrEmpty(x?.ToString())));
    }
}