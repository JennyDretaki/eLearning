using ELearningApi.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ELearning.API.Models
{
    public class QuizQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string QuestionText { get; set; }
        public int ExtraMaterialId { get; set; }
        [ForeignKey("ExtraMaterialId")]
        public ExtraMaterial Material { get; set; }
        public ICollection<QuizOption> QuestionOptions { get; set; } = new List<QuizOption>();
    }
}
