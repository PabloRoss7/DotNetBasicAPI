using DotNetBasicAPI.Models;
using DotNetBasicAPI.Services;

namespace DotNetBasicAPI.Tests;

public class InMemoryUserStoreTests
{
    [Fact]
    public void Add_AssignsIncrementingIds()
    {
        var store = new InMemoryUserStore();
        var first = new User { Name = "A", Email = "a@x.com" };
        var second = new User { Name = "B", Email = "b@x.com" };

        store.Add(first);
        store.Add(second);

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public void GetByEmail_IsCaseInsensitive()
    {
        var store = new InMemoryUserStore();
        var user = new User { Name = "A", Email = "alice@example.com" };
        store.Add(user);

        Assert.Same(user, store.GetByEmail("ALICE@EXAMPLE.COM"));
    }

    [Fact]
    public void GetByEmail_UnknownEmail_ReturnsNull()
    {
        var store = new InMemoryUserStore();

        Assert.Null(store.GetByEmail("nobody@example.com"));
    }

    [Fact]
    public void Remove_DeletesTheUser()
    {
        var store = new InMemoryUserStore();
        var user = new User { Name = "A", Email = "a@x.com" };
        store.Add(user);

        store.Remove(user);

        Assert.Null(store.GetById(user.Id));
    }
}
