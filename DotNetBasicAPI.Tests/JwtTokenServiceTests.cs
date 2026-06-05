using System.IdentityModel.Tokens.Jwt;
using DotNetBasicAPI.Auth;
using DotNetBasicAPI.Models;
using DotNetBasicAPI.Services;
using Microsoft.Extensions.Options;

namespace DotNetBasicAPI.Tests;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(int expiryMinutes = 60) =>
        new(Options.Create(new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = "test-signing-key-which-is-definitely-long-enough-1234567890",
            ExpiryMinutes = expiryMinutes
        }));

    [Fact]
    public void CreateToken_PutsExpectedClaims()
    {
        var service = CreateService();
        var user = new User { Id = 7, Name = "Alice", Email = "alice@example.com" };

        var (token, _) = service.CreateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("7", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("alice@example.com", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Alice", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal("TestIssuer", jwt.Issuer);
    }

    [Fact]
    public void CreateToken_SetsExpiryFromSettings()
    {
        var service = CreateService(expiryMinutes: 30);
        var user = new User { Id = 1, Name = "Bob", Email = "bob@example.com" };

        var (_, expiresAt) = service.CreateToken(user);

        Assert.True(expiresAt > DateTime.UtcNow.AddMinutes(29));
        Assert.True(expiresAt < DateTime.UtcNow.AddMinutes(31));
    }
}
