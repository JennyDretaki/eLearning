using ELearningApi.Models;
using System.Collections.Generic; 
public class Lesson
{
    public int Id { get; set; }
    public string Name { get; set; } 
    public string TutorialContent { get; set; }
    public virtual ICollection<ExtraMaterial> ExtraMaterials { get; set; } = new List<ExtraMaterial>();
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; set; }
    public string UserId { get; internal set; }
}