using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ELearningApi.Dtos;
using ELearningApi.Models;
using ELearningApi.Data;

namespace ELearningApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _context; 
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("{otherUserId}")]
    public async Task<IActionResult> GetMessages(string otherUserId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var receivedMessages = await _context.Messages
            .Where(m => m.SenderId == otherUserId &&
                        m.ReceiverId == currentUserId &&
                        m.IsRead == false)
            .ToListAsync();

        if (receivedMessages.Any())
        {
            foreach (var message in receivedMessages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
       
        var messages = await _context.Messages
            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
            .OrderBy(m => m.Timestamp)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Text = m.Text,
                Timestamp = m.Timestamp,
                IsEdited = m.IsEdited
            })
            .ToListAsync();

        return Ok(messages);
    }


    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var message = new Message
        {
            SenderId = currentUserId,
            ReceiverId = dto.ReceiverId,
            Text = dto.Text,
            Timestamp = DateTime.UtcNow,
            IsEdited = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var messageDto = new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Text = message.Text,
            Timestamp = message.Timestamp,
            IsEdited = message.IsEdited
        };

        return CreatedAtAction(nameof(GetMessages), new { otherUserId = dto.ReceiverId }, messageDto);
    }

    [HttpPut("{messageId}")]
    public async Task<IActionResult> EditMessage(int messageId, [FromBody] string newText)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var message = await _context.Messages.FindAsync(messageId);

        if (message == null) return NotFound();

        if (message.SenderId != currentUserId) return Forbid("You can only edit your own messages.");

        message.Text = newText;
        message.IsEdited = true;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var message = await _context.Messages.FindAsync(messageId);

        if (message == null) return NotFound();


        if (message.SenderId != currentUserId) return Forbid("You can only delete your own messages.");

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Φέρνει το τελευταίο μήνυμα από κάθε συνομιλία του χρήστη
        var conversations = await _context.Messages
            .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
            .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
            .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
            .ToListAsync();

        return Ok(conversations);
    }
    
}