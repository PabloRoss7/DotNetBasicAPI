using System.Net;
using System.Net.Http.Json;

namespace DotNetBasicAPI.Tests;

public class AuthEndpointsTests
{
    [Fact]
    public async Task Register_NewUser_Returns201()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { name = "Alice", email = "alice@example.com", password = "supersecret" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var payload = new { name = "Alice", email = "alice@example.com", password = "supersecret" };

        await client.PostAsJsonAsync("/api/auth/register", payload);
        var second = await client.PostAsJsonAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new { name = "Alice", email = "alice@example.com", password = "supersecret" });

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "alice@example.com", password = "supersecret" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new { name = "Alice", email = "alice@example.com", password = "supersecret" });

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "alice@example.com", password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record TokenResponse(string Token, DateTime ExpiresAtUtc);
}
