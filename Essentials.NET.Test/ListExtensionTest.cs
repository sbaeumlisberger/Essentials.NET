using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Essentials.NET.Test;

public class ListExtensionTest
{

    [Fact]
    public void GetSuccessor_WithList() 
    { 
        var list = new List<int>() { 1, 2, 3, 4, 5 };

        var successor = list.GetSuccessor(2);

        Assert.Equal(3, successor);
    }

    [Fact]
    public void GetSuccessor_WithIList()
    {
        IList<int> list = new List<int>() { 1, 2, 3, 4, 5 };

        var successor = list.GetSuccessor(2);

        Assert.Equal(3, successor);
    }


    [Fact]
    public void GetPredecessor_WithList()
    {
        var list = new List<int>() { 1, 2, 3, 4, 5 };

        var successor = list.GetPredecessor(3);

        Assert.Equal(2, successor);
    }

    [Fact]
    public void GetPredecessor_WithIList()
    {
        IList<int> list = new List<int>() { 1, 2, 3, 4, 5 };

        var successor = list.GetPredecessor(3);

        Assert.Equal(2, successor);
    }
}
