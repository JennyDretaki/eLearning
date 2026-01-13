using System.Threading.Tasks;

namespace ELearningApi.Services
{
    public interface INotificationService
    {
        Task SendGlobalNotificationAsync(string v, string name, int id);
        Task SendGlobalNotificationAsync(string v, object lessonName, int id);
        Task SendQuizResultAsync(string studentId, int quizId, double percentage, string teacherId);

        Task SendQuizResultToAllRelevantAsync(string studentId, int quizId, double percentage, string materialTeacherId);
    }
}