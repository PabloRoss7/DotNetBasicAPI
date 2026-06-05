using DotNetBasicAPI.Models;

namespace DotNetBasicAPI.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}
