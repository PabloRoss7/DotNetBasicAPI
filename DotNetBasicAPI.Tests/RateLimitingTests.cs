using System.Net;

namespace DotNetBasicAPI.Tests;

public class RateLimitingTests
{
    [Fact]
    public async Task ExceedingLimit_Returns429()
    {
        const int permitLimit = 3;
        using var factory = new CustomWebApplicationFactory(permitLimit);
        var client = factory.CreateClient();

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < permitLimit + 2; i++)
        {
            var response = await client.GetAsync("/api/users");
            statuses.Add(response.StatusCode);
        }

        // Los primeros permitLimit pasan; los siguientes se rechazan con 429.
        Assert.All(statuses.Take(permitLimit), s => Assert.Equal(HttpStatusCode.OK, s));
        Assert.Contains(HttpStatusCode.TooManyRequests, statuses);
    }
}
