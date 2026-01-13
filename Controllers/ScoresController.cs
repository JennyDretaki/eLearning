using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ELearningApi.Data;
using ELearningApi.Models;
using ELearning.API.Dtos;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace ELearningApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ScoresController : ControllerBase
{
    private readonly AppDbContext _context;

    public ScoresController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("average")]
    public async Task<IActionResult> GetAverageScore()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var scores = await _context.Scores.Where(s => s.UserId == userId).ToListAsync();
        if (!scores.Any()) return Ok(0);

        double average = scores.Average(s => s.Percentage);
        return Ok(average);
    }

    [HttpGet("{lessonId}/students")]
    [Authorize(Roles = "Admin,Lecturer")]
    public async Task<IActionResult> GetStudentScores(int lessonId)
    {
        var scores = await _context.Scores
            .Where(s => s.LessonId == lessonId && s.IsExtraQuiz == false)
            .Include(s => s.User)
            .ToListAsync();

        var results = new List<object>();

        foreach (var s in scores)
        {
            var mistakes = await _context.UserResponses
                .Where(ur => ur.UserId == s.UserId && ur.Question.LessonId == lessonId && !ur.IsCorrect)
                .Include(ur => ur.Question)
                .Select(ur => new { QuestionText = ur.Question.Text, Chosen = ur.ChosenAnswer, Correct = ur.Question.CorrectAnswer })
                .ToListAsync();

            results.Add(new
            {
                UserId = s.UserId,
                Username = s.User?.UserName,
                Percentage = s.Percentage,
                Mistakes = mistakes
            });
        }

        return Ok(results);
    }

    [HttpGet("extra-quiz-results")]
    [Authorize(Roles = "Admin,Lecturer")]
    public async Task<IActionResult> GetExtraQuizResults([FromQuery] int? lessonId, [FromQuery] int? quizId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        bool isLecturer = string.Equals(userRole, "Lecturer", StringComparison.OrdinalIgnoreCase);

        if (!lessonId.HasValue && !quizId.HasValue)
        {
            return BadRequest("LessonId or QuizId must be provided.");
        }

        var query = _context.Scores
            .Where(s => s.IsExtraQuiz == true);


        if (quizId.HasValue)
        {
            if (isLecturer)
            {
                var quizMaterialCheck = await _context.Set<ExtraMaterial>()
                                                      .Where(em => em.Id == quizId.Value)
                                                      .Select(em => em.TeacherId)
                                                      .FirstOrDefaultAsync();

                if (quizMaterialCheck == null) return NotFound($"Quiz with ID {quizId.Value} not found.");

                if (quizMaterialCheck != currentUserId)
                {
                    return Unauthorized("You are not authorized to view the scores for this specific quiz.");
                }
            }

            query = query.Where(s => (s.ExtraQuizMaterialId.HasValue && s.ExtraQuizMaterialId.Value == quizId.Value) ||
                                     (s.LessonId == quizId.Value));
        }

        else if (lessonId.HasValue)
        {
            var quizIdsInLessonQuery = _context.Set<ExtraMaterial>()
                                               .Where(em => em.LessonId == lessonId.Value && em.IsQuiz);

            if (isLecturer)
            {
                quizIdsInLessonQuery = quizIdsInLessonQuery.Where(em => em.TeacherId == currentUserId);
            }

            var quizIdsInLesson = await quizIdsInLessonQuery
                                                .Select(em => em.Id)
                                                .ToListAsync();

            if (!quizIdsInLesson.Any())
            {
                if (isLecturer)
                {
                    return NotFound($"No extra quiz results found created by you for lesson {lessonId.Value}.");
                }
                return NotFound("No extra quizzes found for this lesson.");
            }

            query = query.Where(s =>
                (s.ExtraQuizMaterialId.HasValue && quizIdsInLesson.Contains(s.ExtraQuizMaterialId.Value)) ||
                (quizIdsInLesson.Contains(s.LessonId))
            );
        }

        var results = await query
     .OrderByDescending(s => s.SubmittedAt)
     .Include(s => s.User)
     .Join(_context.Set<ExtraMaterial>(),
         s => s.ExtraQuizMaterialId.HasValue ? s.ExtraQuizMaterialId.Value : s.LessonId,
         em => em.Id,
         (s, em) => new
         {
             UserId = s.UserId,
             StudentName = s.User != null ? s.User.UserName : "N/A",
             Percentage = s.Percentage,
             SubmittedAt = s.SubmittedAt,
             QuizMaterialId = s.ExtraQuizMaterialId.HasValue ? s.ExtraQuizMaterialId.Value : s.LessonId,
             QuizTitle = em.Title
         }
     )
     .ToListAsync();

        if (!results.Any())
        {
            return NotFound("No extra quiz results found for the specified criteria.");
        }

        return Ok(results.Select(r => new
        {
            userId = r.UserId,
            studentName = r.StudentName,
            percentage = r.Percentage,
            submittedAt = r.SubmittedAt,
            lessonId = r.QuizMaterialId,
            quizTitle = r.QuizTitle
        }).ToList());
    }
}