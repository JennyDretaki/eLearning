using System;
using System.Collections.Generic;

namespace ELearningApi.Dtos
{
    
    public class ExtraQuizListItemDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string Title { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsQuiz { get; set; }
        public string TeacherId { get; set; }
    }

    
}