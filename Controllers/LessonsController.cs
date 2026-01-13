using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningApi.Data;
using ELearningApi.Dtos;
using ELearningApi.Models;
using System.Security.Claims;
using ELearning.API.Models;
using System.Collections.Generic;
using ELearning.API.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using ELearningApi.Services;

namespace ELearningApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LessonsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;
    public LessonsController(AppDbContext context, INotificationService notificationService)
    {
        _context = context;

        _notificationService = notificationService;
    }

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> GetLessons()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized("User identity claim missing.");

        string userId = userIdClaim;
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        bool isStudent = string.Equals(userRole, "Student", StringComparison.OrdinalIgnoreCase);

        var lessonsList = await _context.Lessons.ToListAsync();

        // If the user is NOT a student (Admin/Lecturer), return the list with TutorialContent
        if (!isStudent)
        {
            var adminResults = lessonsList.Select(lesson => new
            {
                Id = lesson.Id.ToString(),
                lesson.Name,
                lesson.TutorialContent, // Added to fix the "not showing" issue
                Progress = 0
            }).ToList();
            return Ok(adminResults);
        }

        // --- Student Logic (Consolidated to remove duplicates) ---

        // 1. Fetch exact scores first
        var exactScores = await _context.Scores
            .Where(s => s.UserId == userId)
            .GroupBy(s => s.LessonId)
            .ToDictionaryAsync(g => g.Key, g => g.Average(x => (double)x.Percentage));

        // 2. If no exact scores found, try normalized matching (removes the duplicate score check)
        if (exactScores.Count == 0)
        {
            var normalizedUserId = userId.Replace("-", "").ToLower();
            exactScores = await _context.Scores
                .Where(s => s.UserId.Replace("-", "").ToLower() == normalizedUserId)
                .GroupBy(s => s.LessonId)
                .ToDictionaryAsync(g => g.Key, g => g.Average(x => (double)x.Percentage));
        }

        // 3. Generate final student results
        var studentResults = lessonsList.Select(lesson => new
        {
            Id = lesson.Id.ToString(),
            lesson.Name,
            Progress = Math.Round(exactScores.GetValueOrDefault(lesson.Id, 0), 2)
        }).ToList();

        return Ok(studentResults);
    }

    [HttpGet("{id}/tutorial")]
    public async Task<IActionResult> GetTutorial(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();
        return Ok(lesson.TutorialContent);
    }

    [HttpGet("{id}/quiz")]
    [Authorize(Roles = "Admin,Student")]
    public async Task<IActionResult> GetQuiz(int id)
    {
        var questions = await _context.Questions.Where(q => q.LessonId == id).ToListAsync();
        return Ok(questions);
    }

    [HttpGet("{id}/lecturer-material")]
    public async Task<IActionResult> GetExtraMaterial(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound($"Lesson with ID {id} not found.");

        var materials = await _context.Set<ExtraMaterial>()
            .Where(m => m.LessonId == id && (m.ExpiryDate == null || m.ExpiryDate >= DateTime.UtcNow))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        if (!materials.Any())
        {
            return Ok(new
            {
                Title = lesson.Name + " Extra Material",
                Sections = new List<object>()
            });
        }

        var sections = materials.Select(m => new
        {
            Id = m.Id.ToString(),
            Title = m.Title,
            Content = m.Content
        }).ToList();


        return Ok(new
        {
            Title = lesson.Name + " Extra Material",
            Sections = sections
        });
    }

    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> CreateLesson([FromBody] LessonDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Προσθήκη CreatedAt για να το βλέπει ο NotificationsController
        var lesson = new Lesson
        {
            Name = dto.Name,
            TutorialContent = dto.TutorialContent,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        // Κλήση της υπηρεσίας ειδοποιήσεων
        await _notificationService.SendGlobalNotificationAsync("new_lesson", lesson.Name, lesson.Id);

        return CreatedAtAction(nameof(GetTutorial), new { id = lesson.Id }, new { Id = lesson.Id, lesson.Name });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] LessonDto dto)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();

        lesson.Name = dto.Name;
        lesson.TutorialContent = dto.TutorialContent;
        // Ενημέρωση του UpdatedAt για να εμφανιστεί στο NotificationBar
        lesson.UpdatedAt = DateTime.UtcNow;

        _context.Entry(lesson).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        await _notificationService.SendGlobalNotificationAsync("lesson_updated", lesson.Name, lesson.Id);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();

        _context.Lessons.Remove(lesson);
        var questions = await _context.Questions.Where(q => q.LessonId == id).ToListAsync();
        _context.Questions.RemoveRange(questions);

        await _context.SaveChangesAsync();
        await _notificationService.SendGlobalNotificationAsync("lesson_deleted", lesson.Name, id);

        return NoContent();
    }

    [HttpPost("extra-material")]
    [Authorize(Roles = "Admin,Lecturer")]
    public async Task<IActionResult> AddExtraMaterial([FromBody] ExtraMaterialDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);


        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (teacherId == null) return Unauthorized("Teacher identity claim missing.");

        var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == dto.LessonId);
        if (!lessonExists) return NotFound("Selected Lesson not found.");

        var material = new ExtraMaterial
        {
            LessonId = dto.LessonId,
            TeacherId = teacherId,
            Title = dto.Title,
            Content = dto.Content,
            ExpiryDate = (DateTime)dto.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };


        var dbSet = _context.Set<ExtraMaterial>();
        await dbSet.AddAsync(material);
        await _context.SaveChangesAsync();


        return Ok(new
        {
            Message = $"Material added successfully for Lesson {dto.LessonId} (Expires {dto.ExpiryDate:yyyy-MM-dd})"
        });
    }

    [HttpGet("ExtraQuizzes/{lessonId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ExtraQuizListItemDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExtraQuizzesByLesson(int lessonId)
    {
        var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == lessonId);
        if (!lessonExists)
        {
            return NotFound($"Lesson with ID {lessonId} not found.");
        }

        var extraQuizzes = await _context.ExtraMaterials
            .Where(em => em.LessonId == lessonId && em.IsQuiz)
            .Select(em => new ExtraQuizListItemDto
            {
                Id = em.Id,
                LessonId = em.LessonId,
                Title = em.Title,
                ExpiryDate = em.ExpiryDate,
                IsQuiz = em.IsQuiz,
                TeacherId = em.TeacherId
            })
            .ToListAsync();

        return Ok(extraQuizzes);
    }

    [HttpGet("ExtraQuizzes/Details/{quizId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExtraQuizDetailsDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExtraQuizDetails(int quizId)
    {
        var quiz = await _context.ExtraMaterials
            .Where(em => em.Id == quizId && em.IsQuiz)
            .FirstOrDefaultAsync();

        if (quiz == null)
        {
            return NotFound($"Extra Quiz with ID {quizId} not found or is not a quiz.");
        }

        var questions = await _context.Set<QuizQuestion>()
            .Include(qq => qq.QuestionOptions)
            .Where(qq => qq.ExtraMaterialId == quizId)
            .ToListAsync();

        var extraQuizDto = new ExtraQuizDetailsDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            ExpiryDate = quiz.ExpiryDate,
            TeacherId = quiz.TeacherId,
            Questions = questions.Select(qq => new QuizQuestionDto
            {
                Id = qq.Id,
                QuestionText = qq.QuestionText,
                Options = qq.QuestionOptions.Select(qo => new QuizOptionDto
                {
                    Text = qo.Text,
                    IsCorrect = qo.IsCorrect
                }).ToList()
            }).ToList()
        };

        return Ok(extraQuizDto);
    }

    [HttpPost("create-quiz")]
    [Authorize(Roles = "Admin,Lecturer")]
    public async Task<IActionResult> CreateQuiz([FromBody] QuizCreationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (teacherId == null) return Unauthorized("Teacher identity claim missing from token.");

        var utcNow = DateTime.UtcNow;
        var defaultExpiryUtc = DateTime.SpecifyKind(new DateTime(2099, 1, 1, 0, 0, 0), DateTimeKind.Utc);

        var material = new ExtraMaterial
        {
            LessonId = dto.LessonId,
            TeacherId = teacherId,
            Title = dto.Title,
            Content = "",
            IsQuiz = true,
            CreatedAt = utcNow,
            ExpiryDate = dto.ExpiryDate.HasValue
                ? DateTime.SpecifyKind(dto.ExpiryDate.Value, DateTimeKind.Utc)
                : defaultExpiryUtc
        };

        _context.ExtraMaterials.Add(material);
        await _context.SaveChangesAsync();

        if (dto.QuizQuestions != null && dto.QuizQuestions.Any())
        {
            foreach (var qDto in dto.QuizQuestions)
            {
                var question = new QuizQuestion
                {
                    QuestionText = qDto.QuestionText,
                    ExtraMaterialId = material.Id,
                };

                foreach (var oDto in qDto.Options)
                {
                    question.QuestionOptions.Add(new QuizOption
                    {
                        Text = oDto.Text,
                        IsCorrect = oDto.IsCorrect
                    });
                }

                _context.QuizQuestions.Add(question);
            }
            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            Id = material.Id,
            Title = material.Title,
            Message = $"Quiz Material created successfully for Lesson {dto.LessonId}"
        });
    }
    [HttpPost("submit-score")]
    [Authorize(Policy = "StudentPolicy")]
    public async Task<IActionResult> SubmitScore([FromBody] ScoreSubmissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User identity claim missing.");

        bool isLessonQuiz = await _context.Lessons.AnyAsync(l => l.Id == dto.LessonId && !dto.IsExtraQuiz);
        bool isExtraQuiz = dto.IsExtraQuiz && await _context.ExtraMaterials.AnyAsync(em => em.Id == dto.LessonId && em.IsQuiz);

        if (!isLessonQuiz && !isExtraQuiz)
        {
            return NotFound("Lesson or Extra Quiz not found.");
        }

        if (dto.Percentage < 0 || dto.Percentage > 100)
        {
            return BadRequest("Percentage must be between 0 and 100.");
        }

        var score = new Score
        {
            UserId = userId,
            LessonId = dto.LessonId,
            Percentage = dto.Percentage,
            IsExtraQuiz = dto.IsExtraQuiz,
            SubmittedAt = DateTime.UtcNow
        };

        _context.Scores.Add(score);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLessons), new { id = dto.LessonId }, new { Message = "Score submitted successfully." });
    }

    [HttpPost("submit-extra-quiz")]
    [Authorize(Policy = "StudentPolicy")]
    public async Task<IActionResult> SubmitExtraQuiz([FromBody] ExtraQuizSubmissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User identity claim missing.");

        var material = await _context.ExtraMaterials
            .FirstOrDefaultAsync(em => em.Id == dto.QuizMaterialId && em.IsQuiz);

        if (material == null)
        {
            return NotFound($"Extra Quiz Material with ID {dto.QuizMaterialId} not found.");
        }

        string notificationTarget = dto.NotificationTargetId;

        var quizQuestions = await _context.QuizQuestions
            .Include(q => q.QuestionOptions)
            .Where(q => q.ExtraMaterialId == dto.QuizMaterialId)
            .ToListAsync();

        int totalQuestions = quizQuestions.Count;
        int correctAnswers = 0;

        foreach (var userAnswer in dto.Answers)
        {
            var question = quizQuestions.FirstOrDefault(q => q.Id == userAnswer.QuizQuestionId);

            if (question != null)
            {
                var correctAnswerOption = question.QuestionOptions.FirstOrDefault(o => o.IsCorrect);

                if (correctAnswerOption != null)
                {
                    if (string.Equals(
                            userAnswer.ChosenAnswer?.Trim(),
                            correctAnswerOption.Text?.Trim(),
                            StringComparison.OrdinalIgnoreCase
                        ))
                    {
                        correctAnswers++;
                    }
                }
            }
        }

        double percentage = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0;
        int finalPercentage = (int)Math.Round(percentage);

        var existingScore = await _context.Scores
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ExtraQuizMaterialId == dto.QuizMaterialId && s.IsExtraQuiz);

        var scoreEntry = new Score
        {
            UserId = userId,
            LessonId = material.LessonId,
            Percentage = finalPercentage,
            IsExtraQuiz = true,
            ExtraQuizMaterialId = dto.QuizMaterialId,
            SubmittedAt = DateTime.UtcNow
        };

        if (existingScore != null)
        {
            existingScore.Percentage = scoreEntry.Percentage;
            existingScore.SubmittedAt = DateTime.UtcNow;
            existingScore.ExtraQuizMaterialId = scoreEntry.ExtraQuizMaterialId;
            _context.Scores.Update(existingScore);
        }
        else
        {
            _context.Scores.Add(scoreEntry);
        }

        if (!string.IsNullOrEmpty(notificationTarget) && notificationTarget != "all")
        {
            await _notificationService.SendQuizResultAsync(userId, material.Id, percentage, notificationTarget);
        }
        else if (notificationTarget == "all")
        {
            await _notificationService.SendQuizResultToAllRelevantAsync(userId, material.Id, percentage, material.TeacherId);
        }

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new
            {
                percentage = percentage,
                lessonId = material.LessonId,
                message = "Score submitted successfully and score saved.",
                notificationTarget = notificationTarget
            });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, $"An error occurred while saving the score: {ex.InnerException?.Message ?? ex.Message}");
        }
    }
}