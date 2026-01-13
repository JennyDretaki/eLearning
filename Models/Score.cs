namespace ELearningApi.Models;

public class Score
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int LessonId { get; set; }
    public double Percentage { get; set; }
    public DateTime CompletedAt { get; set; }
    public ApplicationUser User { get; set; }
    public Lesson Lesson { get; set; }
    public bool IsExtraQuiz { get; internal set; }
    public DateTime SubmittedAt { get; internal set; }
    public int? ExtraQuizMaterialId { get; set; }
}
