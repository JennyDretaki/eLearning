using System.ComponentModel.DataAnnotations;

namespace ELearningApi.Dtos
{

    public class QuizSubmissionDto
    {
        [Required]
        public int LessonId { get; set; }

        [Required]
        [MinLength(15)]
        [MaxLength(15)]
        public List<AnswerDto> Answers { get; set; }
    }

    public class AnswerDto
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string ChosenAnswer { get; set; }
    }
}