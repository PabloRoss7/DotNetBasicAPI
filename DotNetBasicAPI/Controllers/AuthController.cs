using DotNetBasicAPI.Dtos;
using DotNetBasicAPI.Models;
using DotNetBasicAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBasicAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserStore _users;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(IUserStore users, IPasswordHasher<User> passwordHasher, ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterRequest req)
    {
        if (_users.GetByEmail(req.Email) is not null)
            return Conflict(new { message = "El email ya está registrado." });

        var user = new User { Name = req.Name, Email = req.Email };
        user.PasswordHash = _passwordHasher.HashPassword(user, req.Password);
        _users.Add(user);

        var response = new UserResponse { Id = user.Id, Name = user.Name, Email = user.Email };
        return CreatedAtAction("GetById", "Users", new { id = user.Id }, response);
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest req)
    {
        var user = _users.GetByEmail(req.Email);

        // Mensaje genérico: no revelamos si el email existe o si la contraseña falló.
        var invalid_response = Unauthorized(new { message = "Credenciales inválidas." });
        if (user is null) return invalid_response;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed) return invalid_response;

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return Ok(new AuthResponse { Token = token, ExpiresAtUtc = expiresAt });
    }
}
