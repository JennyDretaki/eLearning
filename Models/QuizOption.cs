// QuizOption.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ELearning.API.Models;

namespace ELearning.API.Models
{
    public class QuizOption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public bool IsCorrect { get; set; }

        public int QuizQuestionId { get; set; }

        [ForeignKey("QuizQuestionId")]
        public QuizQuestion QuizQuestion { get; set; }

    }
}