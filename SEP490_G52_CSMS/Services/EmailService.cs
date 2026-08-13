using Microsoft.Extensions.Options;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace SEP490_G52_CSMS.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
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

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("Sending email to {ToEmail} with subject '{Subject}'", toEmail, subject);

            if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer))
            {
                _logger.LogWarning("SMTP Server is not configured. Email body: {Body}", body);
                // In Development/offline mode, we write the email details to Console/Trace for testing
                Console.WriteLine($"[EMAIL FALLBACK] To: {toEmail}\nSubject: {subject}\nBody: {body}");
                return;
            }

            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = _emailSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SMTP to {ToEmail}. Fallback to console log.", toEmail);
                Console.WriteLine($"[EMAIL FALLBACK - ERROR] To: {toEmail}\nSubject: {subject}\nBody: {body}");
                // We rethrow in production but for testability, we log it.
                // We don't crash the app if it fails during manual testing, but since it's a verification step:
                throw new Exception($"Không thể gửi email xác thực: {ex.Message}", ex);
            }
        }
    }
}
