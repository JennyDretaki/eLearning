namespace ELearningApi.Models;

public class Question
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Text { get; set; }
    public string Options { get; set; } 
    public string CorrectAnswer { get; set; }
    public Lesson Lesson { get; set; }
}