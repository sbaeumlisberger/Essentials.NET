using System.Globalization;
using Xunit;

namespace Essentials.NET.Test;

public class ByteSizeFormatterTest
{
    public ByteSizeFormatterTest()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Fact]
    public void Format_HandlesDifferentSizes()
    {
        Assert.Equal("0 Byte", ByteSizeFormatter.Format(0));
        Assert.Equal("1 Byte", ByteSizeFormatter.Format(1));
        Assert.Equal("2 Bytes", ByteSizeFormatter.Format(2));
        Assert.Equal("10 Bytes", ByteSizeFormatter.Format(10));
        Assert.Equal("100 Bytes", ByteSizeFormatter.Format(100));
        Assert.Equal("1000 Bytes", ByteSizeFormatter.Format(1000));

        Assert.Equal("1.00 kB", ByteSizeFormatter.Format(1024));
        Assert.Equal("1.00 MB", ByteSizeFormatter.Format((ulong)Math.Pow(1024, 2)));
        Assert.Equal("1.00 GB", ByteSizeFormatter.Format((ulong)Math.Pow(1024, 3)));
        Assert.Equal("1.00 TB", ByteSizeFormatter.Format((ulong)Math.Pow(1024, 4)));
        Assert.Equal("1.00 PB", ByteSizeFormatter.Format((ulong)Math.Pow(1024, 5)));
        Assert.Equal("1024.00 PB", ByteSizeFormatter.Format((ulong)Math.Pow(1024, 6)));

        Assert.Equal("123.00 MB", ByteSizeFormatter.Format(123 * (ulong)Math.Pow(1024, 2)));

        Assert.Equal("1.50 MB", ByteSizeFormatter.Format((ulong)(1.5 * Math.Pow(1024, 2))));
        Assert.Equal("1.55 MB", ByteSizeFormatter.Format((ulong)(1.55 * Math.Pow(1024, 2))));
        Assert.Equal("1.55 MB", ByteSizeFormatter.Format((ulong)(1.555 * Math.Pow(1024, 2))));
    }

    [Fact]
    public void Format_RespectsCurrentCultureInfo()
    {
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        Assert.Equal("1,00 MB", ByteSizeFormatter.Format((ulong)Math.Pow(1024, 2), 2));
    }

    [Fact]
    public void Format_RespectsCultureInfoParameter()
    {
        Assert.Equal("1,00 MB", ByteSizeFormatter.Format((ulong)Math.Pow(1024, 2), 2, new CultureInfo("de-DE")));
    }

    [Fact]
    public void Format_RespectsDecimalsParameter()
    {
        ulong numberOfBytes = (ulong)Math.Pow(1024, 2);
        Assert.Equal("1 MB", ByteSizeFormatter.Format(numberOfBytes, 0));
        Assert.Equal("1.0 MB", ByteSizeFormatter.Format(numberOfBytes, 1));
        Assert.Equal("1.000 MB", ByteSizeFormatter.Format(numberOfBytes, 3));
    }
}
