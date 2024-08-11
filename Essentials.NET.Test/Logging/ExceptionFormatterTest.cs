using Essentials.NET.Logging;
using System.Diagnostics;
using Xunit;

namespace Essentials.NET.Test.Logging;

public class ExceptionFormatterTest
{
    private class TestException : Exception
    {
        public TestException(string message) : base(message) { }

        public TestException(string message, Exception e) : base(message, e) { }
    }

    private Exception GetThrownTestException()
    {
        try
        {
            throw new TestException("Test Message");
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private Exception GetThrownAggregateException()
    {
        try
        {
            Enumerable.Range(0, 3).AsParallel().Select<int, object>((_, _) =>
            {
                throw new TestException("Test Message");
            }).ToArray();
            throw new UnreachableException();
        }
        catch (Exception e)
        {
            return e;
        }
    }


    private Exception GetThrownTestExceptionCausedByOtherException()
    {
        try
        {
            try
            {
                throw new TestException("Inner Inner Exception Message");
            }
            catch (Exception e1)
            {
                try
                {
                    throw new TestException("Inner Exception Message", e1);
                }
                catch (Exception e2)
                {
                    throw new TestException("Test Message", e2);
                }
            }
        }
        catch (Exception e3)
        {
            return e3;
        }
    }

    [Fact]
    public void Format_TestException()
    {
        string text = ExceptionFormatter.Format(GetThrownTestException()).ReplaceLineEndings("\n");

        Assert.StartsWith("Essentials.NET.Test.Logging.ExceptionFormatterTest+TestException: Test Message\n  at Essentials.NET.Test.Logging.ExceptionFormatterTest.GetThrownTestException()", text);
        Assert.Contains(@"Essentials.NET.Test\Logging\ExceptionFormatterTest.cs:line ", text);
    }

    [Fact]
    public void Format_AggregateException()
    {
        string text = ExceptionFormatter.Format(GetThrownAggregateException()).ReplaceLineEndings("\n");

        Assert.StartsWith("System.AggregateException: One or more errors occurred. (Test Message) (Test Message) (Test Message)\n  at System.Linq.Parallel", text);
        Assert.Contains(@"Essentials.NET.Test\Logging\ExceptionFormatterTest.cs:line ", text);

        Assert.Contains("\nInnerExceptions:\n[0] Essentials.NET.Test.Logging.ExceptionFormatterTest+TestException: Test Message\n    at Essentials.NET.Test.Logging.ExceptionFormatterTest", text);
        Assert.Contains("\n[1] Essentials.NET.Test.Logging.ExceptionFormatterTest+TestException: Test Message\n    at Essentials.NET.Test.Logging.ExceptionFormatterTest", text);
        Assert.Contains("\n[2] Essentials.NET.Test.Logging.ExceptionFormatterTest+TestException: Test Message\n    at Essentials.NET.Test.Logging.ExceptionFormatterTest", text);
    }

    [Fact]
    public void Format_TestExceptionCausedByOtherException()
    {
        string text = ExceptionFormatter.Format(GetThrownTestExceptionCausedByOtherException()).ReplaceLineEndings("\n");

        Assert.StartsWith("Essentials.NET.Test.Logging.ExceptionFormatterTest+TestException: Test Message\n  at Essentials.NET.Test.Logging.ExceptionFormatterTest.GetThrownTestExceptionCausedByOtherException()", text);
        Assert.Contains(@"Essentials.NET.Test\Logging\ExceptionFormatterTest.cs:line ", text);

        Assert.Contains("\nInnerException: Essentials.NET.Test.Logging.ExceptionFormatterTest+TestException: Inner Exception Message\n  at Essentials.NET.Test.Logging.ExceptionFormatterTest.GetThrownTestExceptionCausedByOtherException()", text);
        Assert.Contains(@"Essentials.NET.Test\Logging\ExceptionFormatterTest.cs:line ", text);

        Assert.Contains("\nInnerException: Essentials.NET.Test.Logging.ExceptionFormatterTest+TestException: Inner Inner Exception Message\n  at Essentials.NET.Test.Logging.ExceptionFormatterTest.GetThrownTestExceptionCausedByOtherException()", text);
        Assert.Contains(@"Essentials.NET.Test\Logging\ExceptionFormatterTest.cs:line ", text);

    }

}
