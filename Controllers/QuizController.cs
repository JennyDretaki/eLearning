using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ELearningApi.Data;
using ELearningApi.Dtos;
using ELearningApi.Models;
using System.Reflection;
using System.Security.Principal;

namespace ELearningApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "StudentPolicy")]
[EnableRateLimiting("fixed")]
public class QuizController : ControllerBase
{
    private readonly AppDbContext _context;

    public QuizController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

      
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User identity claim missing from token.");

        var questions = await _context.Questions
                                    .Where(q => q.LessonId == dto.LessonId)
                                    .ToListAsync();

        
        int correctCount = 0;
        int totalAnswered = dto.Answers.Count; 

        foreach (var answer in dto.Answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) continue;

            string chosenClean = answer.ChosenAnswer.Replace(" ", "").Trim();
            string correctClean = question.CorrectAnswer.Replace(" ", "").Trim();

            bool isCorrect = correctClean == chosenClean;
            correctCount += isCorrect ? 1 : 0;

            _context.UserResponses.Add(new UserResponse
            {
                UserId = userId,
                QuestionId = answer.QuestionId,
                ChosenAnswer = answer.ChosenAnswer,
                IsCorrect = isCorrect
            });
        }

        
        if (totalAnswered == 0) totalAnswered = 1;

        double percentage = (correctCount / (double)totalAnswered) * 100;

        _context.Scores.Add(new Score
        {
            UserId = userId,
            LessonId = dto.LessonId,
            Percentage = percentage,
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(new { Percentage = percentage });
    }
    
   
}