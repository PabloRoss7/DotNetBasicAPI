using System.Text;
using System.Threading.RateLimiting;
using DotNetBasicAPI.Auth;
using DotNetBasicAPI.Middleware;
using DotNetBasicAPI.Models;
using DotNetBasicAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

builder.Services.AddAuthorization();

// Límites configurables (sección RateLimiting); defaults 10 req / 10 s por IP.
var permitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 10);
var windowSeconds = builder.Configuration.GetValue("RateLimiting:WindowSeconds", 10);

builder.Services.AddRateLimiter(options =>
{
    // Al superar el límite, responder 429 (por defecto sería 503).
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Límite global: para cada request se decide a qué cupo (partición) pertenece.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // La clave del cupo es la IP del cliente. Esto corre en cada request.
        string clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // El contador para esa IP: ventana fija configurable.
        // La factory corre una vez por IP nueva; el framework reutiliza el contador.
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds)
            });
    });
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Expuesta para que WebApplicationFactory<Program> pueda referenciarla desde los tests.
// public partial class Program { }
// En .NET 10 ya no es necesaria esta linea. Ahora Program es accesible para los tests sin necesidad de definir el partial para que sea public.