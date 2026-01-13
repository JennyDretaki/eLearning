using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ELearningApi.Models;
using ELearning.API.Models;

namespace ELearningApi.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<UserResponse> UserResponses { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<ExtraMaterial> ExtraMaterials { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizOption> QuizOptions { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        builder.Entity<Score>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Score>()
            .HasOne(s => s.Lesson)
            .WithMany() 
            .HasForeignKey(s => s.LessonId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Entity<QuizOption>()
            .HasOne(o => o.QuizQuestion)
            .WithMany(q => q.QuestionOptions)
            .HasForeignKey(o => o.QuizQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}