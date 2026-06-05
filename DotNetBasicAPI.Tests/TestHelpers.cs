using System.Net.Http.Json;

namespace DotNetBasicAPI.Tests;

public static class TestHelpers
{
    // Registra un usuario y hace login; devuelve el JWT para usar en endpoints protegidos.
    public static async Task<string> RegisterAndLoginAsync(
        HttpClient client,
        string name = "User",
        string email = "user@example.com",
        string password = "supersecret")
    {
        await client.PostAsJsonAsync("/api/auth/register", new { name, email, password });

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }

    private record TokenResponse(string Token, DateTime ExpiresAtUtc);
}
