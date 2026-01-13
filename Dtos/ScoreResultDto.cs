namespace ELearning.API.Dtos
{
    internal class ScoreResultDto
    {
        public string UserId { get; set; }
        public double Percentage { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int LessonId { get; set; }
    }
}