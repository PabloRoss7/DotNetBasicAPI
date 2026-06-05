using DotNetBasicAPI.Models;

namespace DotNetBasicAPI.Services;

public interface IUserStore
{
    IReadOnlyList<User> GetAll();
    User? GetById(int id);
    User? GetByEmail(string email);
    void Add(User user);
    void Remove(User user);
}
