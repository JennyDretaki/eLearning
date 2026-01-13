namespace ELearning.API.Dtos
{
    public class ScoreSubmissionDto
    {
        public int LessonId { get; internal set; }
        public bool IsExtraQuiz { get; internal set; }
        public int Percentage { get; internal set; }
    }
}
