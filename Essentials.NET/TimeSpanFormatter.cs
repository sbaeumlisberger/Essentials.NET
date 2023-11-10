namespace Essentials.NET;

public static class TimeSpanFormatter
{

    public static string ToReadableString(TimeSpan timeSpan, TimeUnit accuracy = TimeUnit.Second)
    {
        string readableString = "";

        if (timeSpan.TotalDays < 1 && accuracy == TimeUnit.Day)
        {
            return "< 1d";
        }
        if (timeSpan.Days > 0 && accuracy >= TimeUnit.Day)
        {
            readableString += timeSpan.Days + "d ";
        }

        if (timeSpan.TotalHours < 1 && accuracy == TimeUnit.Hour)
        {
            return "< 1h";
        }
        if (timeSpan.Hours > 0 && accuracy >= TimeUnit.Hour)
        {
            readableString += timeSpan.Hours + "h ";
        }

        if (timeSpan.TotalMinutes < 1 && accuracy == TimeUnit.Minute)
        {
            return "< 1min";
        }

        if (timeSpan.Minutes > 0 && accuracy >= TimeUnit.Minute)
        {
            readableString += timeSpan.Minutes + "min ";
        }

        if (timeSpan.TotalSeconds < 1 && accuracy == TimeUnit.Second)
        {
            return "< 1s";
        }
        if (timeSpan.Seconds > 0 && accuracy >= TimeUnit.Second)
        {
            readableString += timeSpan.Seconds + "s ";
        }

        if (timeSpan.Milliseconds > 0 && accuracy >= TimeUnit.Millisecond)
        {
            readableString += timeSpan.Milliseconds + "ms ";
        }

        return readableString.TrimEnd(' ');
    }

}
