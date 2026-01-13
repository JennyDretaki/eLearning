using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ELearningApi.Dtos
{
    public class ExtraMaterialDto
    {
        [Required]
        public int LessonId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}