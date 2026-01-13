using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ELearning.API.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningApi.Models
{
   
   

    public class ExtraMaterial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public ApplicationUser Teacher { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        public bool IsQuiz { get; set; }

        public DateTime? CreatedAt { get; set; }

        public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
    }
}