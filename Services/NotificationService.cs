using System.Threading.Tasks;
using System.Diagnostics;

namespace ELearningApi.Services
{
    public class NotificationService : INotificationService
    {
        public Task SendGlobalNotificationAsync(string type, string name, int id)
        {
            System.Diagnostics.Debug.WriteLine($"Notification: {type} - {name} (ID: {id})");
            return Task.CompletedTask;
        }

        public Task SendGlobalNotificationAsync(string type, object lessonName, int id)
        {
            System.Diagnostics.Debug.WriteLine($"Notification: {type} - {lessonName} (ID: {id})");
            return Task.CompletedTask;
        }

        public Task SendQuizResultAsync(string studentId, int quizId, double percentage, string teacherId)
        {
            Debug.WriteLine($"Notification sent: Student {studentId} submitted Quiz {quizId} ({percentage}%) to Teacher {teacherId}.");
            return Task.CompletedTask;
        }

        public Task SendQuizResultToAllRelevantAsync(string studentId, int quizId, double percentage, string materialTeacherId)
        {
            Debug.WriteLine($"Notification sent: Student {studentId} submitted Quiz {quizId} ({percentage}%) to all relevant staff.");
            return Task.CompletedTask;
        }
    }
}