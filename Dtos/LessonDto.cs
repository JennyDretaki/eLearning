using System.ComponentModel.DataAnnotations;

namespace ELearningApi.Dtos
{

    public class LessonDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [Required]
        public string? TutorialContent { get; set; }
    }
}