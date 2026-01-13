using System.Threading.Tasks;

namespace ELearningApi.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}