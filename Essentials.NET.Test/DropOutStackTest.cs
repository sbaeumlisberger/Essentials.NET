using Xunit;

namespace Essentials.NET.Test;

public class DropOutStackTest
{

    [Fact]
    public void FullTest()
    {
        var stack = new DropOutStack<string>(5);
        stack.Push("A");
        stack.Push("B");
        stack.Push("C");
        stack.Push("D");
        stack.Push("E");

        Console.WriteLine(string.Join(", ", stack));
        Assert.True(Enumerable.SequenceEqual(new string[] { "E", "D", "C", "B", "A" }, stack));

        stack.Push("F");
        stack.Push("G");

        Console.WriteLine(string.Join(", ", stack));
        Assert.True(Enumerable.SequenceEqual(new string[] { "G", "F", "E", "D", "C" }, stack));

        stack.Clear();

        Console.WriteLine(string.Join(", ", stack));
        Assert.True(Enumerable.SequenceEqual(new string[] { }, stack));

        stack.Push("A");
        stack.Push("B");
        stack.Push("C");

        Console.WriteLine(string.Join(", ", stack));
        Assert.True(Enumerable.SequenceEqual(new string[] { "C", "B", "A" }, stack));

        Assert.Equal("C", stack.Peek());

        stack.Pop();

        Console.WriteLine(string.Join(", ", stack));
        Assert.True(Enumerable.SequenceEqual(new string[] { "B", "A" }, stack));

        stack.Push("X");

        Console.WriteLine(string.Join(", ", stack));
        Assert.True(Enumerable.SequenceEqual(new string[] { "X", "B", "A" }, stack));

        stack.Push("1");
        stack.Push("2");
        stack.Push("3");
        stack.Push("4");
        stack.Push("5");

        Console.WriteLine(string.Join(", ", stack));
        Assert.True(Enumerable.SequenceEqual(new string[] { "5", "4", "3", "2", "1" }, stack));
    }

}