using ELearningApi.Dtos;

namespace ELearning.API.Dtos
{
    public class ExtraQuizDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string TeacherId { get; set; }

        // Χρησιμοποιούμε το QuizQuestionDto που έχετε ήδη
        public List<QuizQuestionDto> Questions { get; set; }
    }
}
