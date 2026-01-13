namespace ELearningApi.Models;

public class UserResponse
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int QuestionId { get; set; }
    public string ? ChosenAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public ApplicationUser User { get; set; }
    public Question Question { get; set; }
}