using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ELearningApi.Dtos
{
    public class ExtraQuizSubmissionDto
    {
        [Required]
        public int QuizMaterialId { get; set; } 

        [Required]
        [MinLength(1)]
        public List<UserAnswerDto> Answers { get; set; }
        [Required]

        public string NotificationTargetId { get; set; }
    }
}