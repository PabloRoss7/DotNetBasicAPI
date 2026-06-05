using System.ComponentModel.DataAnnotations;

namespace DotNetBasicAPI.Dtos;

public class UpdateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
