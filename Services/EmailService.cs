using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ELearningApi.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");

        var client = new SmtpClient(smtpSettings["Server"])
        {
            Port = int.Parse(smtpSettings["Port"]),
            Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
            EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpSettings["FromEmail"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
        };
        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}