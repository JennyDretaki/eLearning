using System;
using System.ComponentModel.DataAnnotations;

namespace ELearningApi.Dtos
{
    public class QuizCreationDto
    {
        [Required]
        public int LessonId { get; set; }

        [Required]
        public string Title { get; set; } 

        public DateTime? ExpiryDate { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "A quiz must contain at least one question.")]
        public List<QuizQuestionDto> QuizQuestions { get; set; }
    }
}