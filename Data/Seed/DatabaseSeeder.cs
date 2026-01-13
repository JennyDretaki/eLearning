using ELearningApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace ELearningApi.Data.Seed;

public static class DatabaseSeeder
{
    private static readonly string[] Roles = new[] { "Admin", "Lecturer", "Student" };

    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        try
        {
            await context.Database.MigrateAsync();

            // 1. Seed Roles
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Ensure Admin exists and get their ID (fixes "UserId violates not-null constraint")
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = "admin", Email = "admin@fynix.com", EmailConfirmed = true };
                var result = await userManager.CreateAsync(adminUser, "A!@#5462xcgf@@#456gccgf");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Upsert Lessons (Updates responsive HTML if it already exists)
            var contentPath = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "Content");

            foreach (var lessonMeta in SeedData.LessonFiles)
            {
                var fullPath = Path.Combine(contentPath, lessonMeta.FileName);

                if (!File.Exists(fullPath)) continue;

                var tutorialContent = await File.ReadAllTextAsync(fullPath, Encoding.UTF8);
                var existingLesson = await context.Lessons.FirstOrDefaultAsync(l => l.Name == lessonMeta.Name);

                if (existingLesson != null)
                {
                    existingLesson.TutorialContent = tutorialContent;
                    existingLesson.UserId = adminUser.Id; // Fix null UserId violation
                    context.Lessons.Update(existingLesson);
                }
                else
                {
                    await context.Lessons.AddAsync(new Lesson
                    {
                        Name = lessonMeta.Name,
                        TutorialContent = tutorialContent,
                        UserId = adminUser.Id // Fix null UserId violation
                    });
                }
            }

            // Save lessons first so they have IDs for the Questions to reference
            await context.SaveChangesAsync();

            // 4. Seed Questions (Fixed Foreign Key Violation)
            // If we deleted Lessons previously, Questions must also be re-seeded to match valid LessonIds
            if (!await context.Questions.AnyAsync())
            {
                var questions = SeedData.GetQuestions();
                foreach (var q in questions)
                {
                    // Map the hardcoded LessonId in SeedData to the actual ID in the DB by Name
                    var lessonName = SeedData.LessonFiles.FirstOrDefault(f => f.Id == q.LessonId).Name;
                    var actualLesson = await context.Lessons.FirstOrDefaultAsync(l => l.Name == lessonName);

                    if (actualLesson != null)
                    {
                        q.LessonId = actualLesson.Id;
                        await context.Questions.AddAsync(q);
                    }
                }
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SEEDING ERROR: {ex.Message}");
            Console.WriteLine(ex.ToString());
        }
    }
}