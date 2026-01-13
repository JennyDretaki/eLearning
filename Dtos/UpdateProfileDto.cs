using System.ComponentModel.DataAnnotations;

namespace ELearningApi.Dtos;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;
    public string? OldPassword { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be 6-100 characters.")]
    public string? NewPassword { get; set; }
}
