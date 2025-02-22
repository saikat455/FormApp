using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace FormApp.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }

    public interface IEmailService
    {
        Task SendFormAnswersEmailAsync(string userEmail, List<(string QuestionTitle, string Answer)> answers, string templateTitle);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendFormAnswersEmailAsync(string userEmail, List<(string QuestionTitle, string Answer)> answers, string templateTitle)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("", userEmail));
                message.Subject = $"Your answers for {templateTitle}";

                var body = new BodyBuilder();
                var htmlContent = new System.Text.StringBuilder();
                htmlContent.AppendLine("<h2>Your Form Answers</h2>");
                htmlContent.AppendLine($"<h3>Template: {templateTitle}</h3>");
                htmlContent.AppendLine("<table style='border-collapse: collapse; width: 100%;'>");
                htmlContent.AppendLine("<tr style='background-color: #f2f2f2;'><th style='border: 1px solid #ddd; padding: 8px;'>Question</th><th style='border: 1px solid #ddd; padding: 8px;'>Your Answer</th></tr>");

                foreach (var (QuestionTitle, Answer) in answers)
                {
                    htmlContent.AppendLine($"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{QuestionTitle}</td><td style='border: 1px solid #ddd; padding: 8px;'>{Answer}</td></tr>");
                }

                htmlContent.AppendLine("</table>");
                body.HtmlBody = htmlContent.ToString();
                message.Body = body.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {UserEmail}", userEmail);
                throw new Exception("Failed to send email. Please check email configuration.", ex);
            }
        }
    }
}