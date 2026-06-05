using DotNetBasicAPI.Dtos;
using DotNetBasicAPI.Models;
using DotNetBasicAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBasicAPI.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserStore _users;

    public UsersController(IUserStore users)
    {
        _users = users;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_users.GetAll().Select(ToResponse));

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var user = _users.GetById(id);
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [Authorize]
    [HttpPost]
    public IActionResult Create(CreateUserRequest req)
    {
        if (_users.GetByEmail(req.Email) is not null)
            return Conflict(new { message = "El email ya está en uso." });

        var user = new User
        {
            Name = req.Name,
            Email = req.Email
        };
        _users.Add(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToResponse(user));
    }

    [Authorize]
    [HttpPut("{id}")]
    public IActionResult Update(int id, UpdateUserRequest req)
    {
        var user = _users.GetById(id);
        if (user is null) return NotFound();

        // El email puede repetir el propio; solo choca si pertenece a OTRO usuario.
        var existing = _users.GetByEmail(req.Email);
        if (existing is not null && existing.Id != id)
            return Conflict(new { message = "El email ya está en uso." });

        user.Name = req.Name;
        user.Email = req.Email;
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var user = _users.GetById(id);
        if (user is null) return NotFound();

        _users.Remove(user);
        return NoContent();
    }

    private static UserResponse ToResponse(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email
    };
}
