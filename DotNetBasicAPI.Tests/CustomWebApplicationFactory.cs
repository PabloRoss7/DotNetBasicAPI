using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotNetBasicAPI.Tests;

// Levanta la app en memoria. Por defecto sube el límite de rate limiting a un
// valor altísimo para que los tests normales no choquen con 429; el test de
// rate limiting crea una factory con un límite chico para ejercitarlo.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly int _permitLimit;

    public CustomWebApplicationFactory(int permitLimit = 100_000)
    {
        _permitLimit = permitLimit;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("RateLimiting:PermitLimit", _permitLimit.ToString());
        builder.UseSetting("RateLimiting:WindowSeconds", "10");
    }
}
