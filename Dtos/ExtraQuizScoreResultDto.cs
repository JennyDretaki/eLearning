 public class ExtraQuizScoreResultDto
{
    public string UserId { get; set; }
    public string StudentName { get; set; }
    public double Percentage { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int QuizMaterialId { get; set; }
    public string QuizTitle { get; set; } 
}