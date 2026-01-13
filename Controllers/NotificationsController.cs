using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ELearningApi.Data;
using ELearningApi.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ELearningApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet("status")]
    public async Task<IActionResult> GetNotificationsStatus()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null)
        {
            return Unauthorized("User identity claim missing.");
        }

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        try
        {
            var notificationEvents = new List<object>();

            var unreadMessages = await _context.Messages
                .Where(m => m.ReceiverId == currentUserId && m.IsRead == false)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.Timestamp)
                .Take(5)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                var senderDisplayName = (msg.Sender?.FirstName ?? msg.Sender?.UserName);

                notificationEvents.Add(new
                {
                    Id = msg.Id,
                    Type = "message",
                    SourceName = senderDisplayName ?? "Unknown User",
                    SourceId = msg.SenderId,
                    IsQuiz = false,
                    Timestamp = msg.Timestamp
                });
            }

            var lessonEvents = await _context.Lessons
                .Where(l => l.CreatedAt >= sevenDaysAgo || (l.UpdatedAt.HasValue && l.UpdatedAt.Value >= sevenDaysAgo))
                .OrderByDescending(l => l.CreatedAt)
                .Take(5) 
                .ToListAsync();

            foreach (var lesson in lessonEvents)
            {
                if (lesson.CreatedAt >= sevenDaysAgo)
                {
                    notificationEvents.Add(new
                    {
                        Id = lesson.Id,
                        Type = "new_lesson",
                        SourceName = lesson.Name,
                        SourceId = lesson.Id,
                        IsQuiz = false,
                        Timestamp = lesson.CreatedAt
                    });
                }

                if (lesson.UpdatedAt.HasValue && lesson.UpdatedAt.Value >= sevenDaysAgo)
                {
                    notificationEvents.Add(new
                    {
                        Id = lesson.Id,
                        Type = "lesson_updated",
                        SourceName = lesson.Name,
                        SourceId = lesson.Id,
                        IsQuiz = false,
                        Timestamp = lesson.UpdatedAt.Value
                    });
                }
            }


            var newMaterials = await _context.Set<ExtraMaterial>()
                .Where(m => m.CreatedAt >= sevenDaysAgo)
                .Include(m => m.Teacher) 
                .OrderByDescending(m => m.CreatedAt)
                .Take(3)
                .ToListAsync();

            foreach (var material in newMaterials)
            {
                var teacherDisplayName = (material.Teacher?.FirstName ?? material.Teacher?.UserName);

                notificationEvents.Add(new
                {
                    Id = material.Id,
                    Type = "material",
                    SourceName = teacherDisplayName ?? "Unknown Teacher",
                    SourceId = material.LessonId,
                    IsQuiz = material.IsQuiz,
                    Timestamp = material.CreatedAt
                });
            }

            var sortedEvents = notificationEvents.OrderByDescending(e => (DateTime)e.GetType().GetProperty("Timestamp").GetValue(e)).ToList();

            return Ok(sortedEvents);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching notification status for user {currentUserId}: {ex.Message}");
            return StatusCode(500, "An error occurred while fetching notification status.");
        }
    }
}