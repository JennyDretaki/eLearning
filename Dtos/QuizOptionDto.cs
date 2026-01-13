using System.ComponentModel.DataAnnotations;

namespace ELearningApi.Dtos
{
    public class QuizOptionDto
    {
        [Required]
        public string Text { get; set; }

        public bool IsCorrect { get; set; }
    }
}