using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ELearningApi.Dtos;
using ELearningApi.Models;
using Microsoft.EntityFrameworkCore;
using ELearningApi.Data;
using System.Diagnostics;

namespace ELearningApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    public UserController(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }


    [HttpGet("list")]
    public async Task<IActionResult> GetUsersForChat()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var allUsers = await _userManager.Users
            .Where(u => u.Id != currentUserId)
            .ToListAsync();

        var userDtos = new List<UserChatDto>();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var unreadCount = await _context.Messages
                  .CountAsync(m => m.SenderId == user.Id &&
                                   m.ReceiverId == currentUserId &&
                                   m.IsRead == false);

            userDtos.Add(new UserChatDto
            {
                Id = user.Id,
                Name = user.UserName,
                Role = roles.FirstOrDefault(),
                UnreadCount = unreadCount
            });
        }

        return Ok(userDtos);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return NotFound("User not found.");


        if (!await _userManager.CheckPasswordAsync(user, dto.OldPassword))
        {
            return BadRequest("Invalid current password.");
        }


        var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

        if (!result.Succeeded)
        {

            return BadRequest(result.Errors);
        }


        if (dto.Username != null && user.UserName != dto.Username)
        {
            user.UserName = dto.Username;
            await _userManager.UpdateAsync(user);
        }
        if (dto.Email != null && user.Email != dto.Email)
        {
            user.Email = dto.Email;
            await _userManager.UpdateAsync(user);
        }

        return NoContent();
    }


    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }

    [HttpGet("health-check")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSystemHealth()
    {
        bool dbConnectionStatus;
        string dbVersion = "N/A";

        try
        {
            dbConnectionStatus = await _context.Database.CanConnectAsync();

            if (dbConnectionStatus)
            {
                await using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    dbVersion = connection.ServerVersion; 
                }
            }
        }
        catch (Exception ex)
        {
            dbConnectionStatus = false;
            Console.WriteLine($"DB connection error: {ex.Message}");
        }

        var process = Process.GetCurrentProcess();
        TimeSpan uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();

        var healthData = new
        {
            ServerStatus = "Online",
            DatabaseConnection = dbConnectionStatus ? "OK" : "ERROR",
            DBVersion = dbVersion,
            Uptime = uptime.ToString(@"dd\.hh\:mm\:ss"),
            SystemTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            RequiredRole = "Admin"
        };

        return Ok(healthData);
    }
}