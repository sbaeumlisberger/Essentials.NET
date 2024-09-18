using System;
using System.Text;
using System.Text.Json;

namespace Essentials.NET.Logging;

internal static class ExceptionFormatter
{
    public static readonly string IndentStep = "  ";

    public static string Format(Exception exception)
    {
        StringBuilder stringBuilder = new StringBuilder();
        AppendException(stringBuilder, exception, "");
        return stringBuilder.ToString();
    }

    private static void AppendException(StringBuilder stringBuilder, Exception exception, string indent)
    {
        stringBuilder.Append(exception.GetType());
        stringBuilder.Append(": ");
        stringBuilder.Append(exception.Message);
        stringBuilder.AppendLine();

        AppendStackTrace(stringBuilder, exception, indent);

        AppendProperties(stringBuilder, exception, indent);

        if (exception is AggregateException aggregateException)
        {
            AppendInnerExceptions(stringBuilder, aggregateException.InnerExceptions, indent);
        }
        else if (exception.InnerException is not null)
        {
            AppendInnerException(stringBuilder, exception.InnerException, indent);
        }
    }

    private static void AppendStackTrace(StringBuilder stringBuilder, Exception exception, string indent)
    {
        if (exception.StackTrace is string stackTrace)
        {
            foreach (string line in stackTrace.Split("\n"))
            {
                stringBuilder.Append(indent);
                stringBuilder.Append(IndentStep);
                stringBuilder.Append(line.Trim());
                stringBuilder.AppendLine();
            }
        }
    }

    private static void AppendProperties(StringBuilder stringBuilder, Exception exception, string indent)
    {
        AppendProperty(stringBuilder, "Source", exception.Source, indent);

        foreach (var key in exception.Data.Keys)
        {
            AppendProperty(stringBuilder, "Data[" + key + "]", exception.Data[key]?.ToString(), indent);
        }

        AppendProperty(stringBuilder, "HelpLink", exception.HelpLink, indent);

        if (exception.HResult != 0)
        {
            AppendProperty(stringBuilder, "HResult", exception.HResult.ToString(), indent);
        }
    }

    private static void AppendProperty(StringBuilder stringBuilder, string propertyName, string? value, string indent)
    {
        if (value is not null)
        {
            stringBuilder.Append(indent);
            stringBuilder.Append(IndentStep);
            stringBuilder.Append(propertyName);
            stringBuilder.Append(": ");
            stringBuilder.Append(value);
            stringBuilder.AppendLine();
        }
    }


    private static void AppendInnerException(StringBuilder stringBuilder, Exception innerException, string indent)
    {
        stringBuilder.Append(indent);
        stringBuilder.Append("InnerException: ");
        AppendException(stringBuilder, innerException, indent);
    }

    private static void AppendInnerExceptions(StringBuilder stringBuilder, IReadOnlyList<Exception> innerExceptions, string indent)
    {
        stringBuilder.Append(indent);
        stringBuilder.Append("InnerExceptions:");
        stringBuilder.AppendLine();

        for (int i = 0; i < innerExceptions.Count; i++)
        {
            stringBuilder.Append(indent);
            stringBuilder.Append("[");
            stringBuilder.Append(i);
            stringBuilder.Append("] ");
            AppendException(stringBuilder, innerExceptions[i], indent + IndentStep);
        }
    }

}
