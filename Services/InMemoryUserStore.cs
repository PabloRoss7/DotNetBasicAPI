using DotNetBasicAPI.Models;

namespace DotNetBasicAPI.Services;

public class InMemoryUserStore : IUserStore
{
    private readonly List<User> _users = [];
    private readonly Lock _gate = new();
    private int _nextId = 1;

    public IReadOnlyList<User> GetAll()
    {
        lock (_gate)
        {
            return _users.ToList();
        }
    }

    public User? GetById(int id)
    {
        lock (_gate)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
    }

    public User? GetByEmail(string email)
    {
        lock (_gate)
        {
            return _users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public void Add(User user)
    {
        lock (_gate)
        {
            user.Id = _nextId++;
            _users.Add(user);
        }
    }

    public void Remove(User user)
    {
        lock (_gate)
        {
            _users.Remove(user);
        }
    }
}
