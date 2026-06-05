using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DotNetBasicAPI.Tests;

public class UsersEndpointsTests
{
    [Fact]
    public async Task GetAll_IsPublic_AndHidesPasswordHash()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await TestHelpers.RegisterAndLoginAsync(client);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users",
            new { name = "Bob", email = "bob@example.com" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithToken_Returns201()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await Authenticate(client);

        var response = await client.PostAsJsonAsync("/api/users",
            new { name = "Bob", email = "bob@example.com" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns409()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await Authenticate(client);
        var payload = new { name = "Bob", email = "bob@example.com" };

        await client.PostAsJsonAsync("/api/users", payload);
        var second = await client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidBody_Returns400()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await Authenticate(client);

        var emptyName = await client.PostAsJsonAsync("/api/users",
            new { name = "", email = "bob@example.com" });
        var badEmail = await client.PostAsJsonAsync("/api/users",
            new { name = "Bob", email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, emptyName.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, badEmail.StatusCode);
    }

    [Fact]
    public async Task Update_WithToken_Returns204()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await Authenticate(client);
        var created = await client.PostAsJsonAsync("/api/users",
            new { name = "Bob", email = "bob@example.com" });
        var user = await created.Content.ReadFromJsonAsync<UserDto>();

        var response = await client.PutAsJsonAsync($"/api/users/{user!.Id}",
            new { name = "Bob Updated", email = "bob@example.com" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_ToAnotherUsersEmail_Returns409()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await Authenticate(client);
        await client.PostAsJsonAsync("/api/users", new { name = "Ann", email = "ann@example.com" });
        var bobResp = await client.PostAsJsonAsync("/api/users", new { name = "Bob", email = "bob@example.com" });
        var bob = await bobResp.Content.ReadFromJsonAsync<UserDto>();

        var response = await client.PutAsJsonAsync($"/api/users/{bob!.Id}",
            new { name = "Bob", email = "ann@example.com" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithToken_Returns204()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await Authenticate(client);
        var created = await client.PostAsJsonAsync("/api/users",
            new { name = "Bob", email = "bob@example.com" });
        var user = await created.Content.ReadFromJsonAsync<UserDto>();

        var response = await client.DeleteAsync($"/api/users/{user!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task Authenticate(HttpClient client)
    {
        var token = await TestHelpers.RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private record UserDto(int Id, string Name, string Email);
}
