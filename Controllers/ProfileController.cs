using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ELearningApi.Data;
using ELearningApi.Models;
using ELearningApi.Dtos;

namespace ELearningApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public ProfileController(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Invalid input data.", Errors = ModelState });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(new { Message = "User identity claim missing." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        if (!string.IsNullOrEmpty(dto.Username) && user.UserName != dto.Username)
        {
            user.UserName = dto.Username;
        }

        if (!string.IsNullOrEmpty(dto.Email) && user.Email != dto.Email)
        {
            var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, dto.Email);
            var emailResult = await _userManager.ChangeEmailAsync(user, dto.Email, emailToken);
            if (!emailResult.Succeeded)
            {
                return BadRequest(new { Message = "Failed to update email.", Errors = emailResult.Errors });
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new { Message = "Failed to update profile.", Errors = updateResult.Errors });
        }

        
        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!resetResult.Succeeded)
            {
                return BadRequest(new { Message = "Failed to update password.", Errors = resetResult.Errors });
            }
        }

        Console.WriteLine($"Profile updated for user {userId}: Username={user.UserName}, Email={user.Email}");

        return Ok(new { Message = "Profile updated successfully." });
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                Title = "One or more validation errors occurred.",
                Errors = ModelState 
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(new { Message = "User identity claim missing." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

         var changeResult = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
        if (!changeResult.Succeeded)
        {
            return BadRequest(new { Message = "Failed to change password.", Errors = changeResult.Errors });
        }

      Console.WriteLine($"Password changed for user {userId}");

        return Ok(new { Message = "Password changed successfully." });
    }
}