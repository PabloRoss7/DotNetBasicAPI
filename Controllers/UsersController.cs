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
