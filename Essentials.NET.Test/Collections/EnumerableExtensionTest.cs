using Xunit;

namespace Essentials.NET.Test.Collections;

public class EnumerableExtensionTest
{

    [Fact]
    public void GetSuccessor_WithIEnumerable()
    {
        IEnumerable<int> enumerable = Enumerable.Range(1, 5);

        var successor = enumerable.GetSuccessor(2);

        Assert.Equal(3, successor);
    }

    [Fact]
    public void GetSuccessor_WithIEnumerable_NoResult()
    {
        IEnumerable<int> enumerable = Enumerable.Range(1, 5);

        var successor = enumerable.GetSuccessor(5);

        Assert.Equal(default, successor);
    }

    [Fact]
    public void GetSuccessor_WithList()
    {
        List<int> list = Enumerable.Range(1, 5).ToList();

        var successor = list.GetSuccessor(2);

        Assert.Equal(3, successor);
    }

    [Fact]
    public void GetSuccessor_WithList_NoResult()
    {
        List<int> list = Enumerable.Range(1, 5).ToList();

        var successor = list.GetSuccessor(5);

        Assert.Equal(default, successor);
    }

    [Fact]
    public void GetSuccessor_WithIList()
    {
        IList<int> list = Enumerable.Range(1, 5).ToList();

        var successor = list.GetSuccessor(2);

        Assert.Equal(3, successor);
    }

    [Fact]
    public void GetSuccessor_WithIReadOnlyList()
    {
        IReadOnlyList<int> list = Enumerable.Range(1, 5).ToList().AsReadonly();

        var successor = list.GetSuccessor(2);

        Assert.Equal(3, successor);
    }

    [Fact]
    public void GetPredecessor_WithIEnumerable()
    {
        IEnumerable<int> list = Enumerable.Range(1, 5);

        var successor = list.GetPredecessor(3);

        Assert.Equal(2, successor);
    }

    [Fact]
    public void GetPredecessor_WithIEnumerable_NoResult()
    {
        IEnumerable<int> list = Enumerable.Range(1, 5);

        var successor = list.GetPredecessor(1);

        Assert.Equal(default, successor);
    }

    [Fact]
    public void GetPredecessor_WithList()
    {
        List<int> list = Enumerable.Range(1, 5).ToList();

        var successor = list.GetPredecessor(3);

        Assert.Equal(2, successor);
    }

    [Fact]
    public void GetPredecessor_WithList_NoResult()
    {
        List<int> list = Enumerable.Range(1, 5).ToList();

        var successor = list.GetPredecessor(1);

        Assert.Equal(default, successor);
    }

    [Fact]
    public void GetPredecessor_WithIList()
    {
        IList<int> list = Enumerable.Range(1, 5).ToList();

        var successor = list.GetPredecessor(3);

        Assert.Equal(2, successor);
    }

    [Fact]
    public void GetPredecessor_WithIReadOnlyList()
    {
        IReadOnlyList<int> list = Enumerable.Range(1, 5).ToList().AsReadonly();

        var successor = list.GetPredecessor(3);

        Assert.Equal(2, successor);
    }
}
