
using System.ComponentModel.DataAnnotations;

namespace ELearningApi.Dtos
{
    public class UserAnswerDto
    {
        [Required]
        public int QuizQuestionId { get; set; } 

        [Required]
        public string ChosenAnswer { get; set; }
    }
}