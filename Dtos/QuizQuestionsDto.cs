using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ELearningApi.Dtos
{
    public class QuizQuestionDto
    {
        [Required]
        public string QuestionText { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "A question must have at least one option.")]
        public List<QuizOptionDto> Options { get; set; }
        public int Id { get; internal set; }
    }
}