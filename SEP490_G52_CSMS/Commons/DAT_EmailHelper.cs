using System.Net.Mail;

namespace SEP490_G52_CSMS.Commons
{
    public interface IDAT_EmailHelper
    {
        void SendAccountCredentials(string recipientEmail, string fullName, string username, string plainPassword);
    }

    public class DAT_EmailHelper : IDAT_EmailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public DAT_EmailHelper(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public void SendAccountCredentials(string recipientEmail, string fullName, string username, string plainPassword)
        {
            string subject = "CSMS - Thong tin tai khoan nhan vien moi";
            string body = $@"Xin chao {fullName},

Tai khoan nhan vien cua ban da duoc khoi tao thanh cong tren he thong Quan ly chuoi cua hang ca phe CSMS.

Thong tin dang nhap cua ban:
- Ten dang nhap (Username): {username}
- Mat khau (Password): {plainPassword}

Vui long dang nhap vao he thong va tien hanh dang ky khuon mat/doi mat khau theo yeu cau.

Tran trong,
CSMS System Administrator";

            try
            {
                // Try sending using configured SMTP if available
                var smtpServer = _configuration["Smtp:Server"];
                var smtpPortStr = _configuration["Smtp:Port"];
                var smtpUser = _configuration["Smtp:Username"];
                var smtpPass = _configuration["Smtp:Password"];

                if (!string.IsNullOrEmpty(smtpServer) && int.TryParse(smtpPortStr, out int smtpPort))
                {
                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress(smtpUser ?? "noreply@csms.com", "CSMS System");
                        mail.To.Add(recipientEmail);
                        mail.Subject = subject;
                        mail.Body = body;

                        using (var smtp = new SmtpClient(smtpServer, smtpPort))
                        {
                            smtp.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                // Log SMTP failure and fallback
                Console.WriteLine($"SMTP delivery failed: {ex.Message}");
            }

            // Fallback: write to a local file in wwwroot
            try
            {
                string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(webRootPath))
                {
                    Directory.CreateDirectory(webRootPath);
                }

                string filePath = Path.Combine(webRootPath, "sent_emails.txt");
                string logContent = $"========================================\n" +
                                    $"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                    $"To: {recipientEmail}\n" +
                                    $"Subject: {subject}\n" +
                                    $"Body:\n{body}\n" +
                                    $"========================================\n\n";

                File.AppendAllText(filePath, logContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write local email log: {ex.Message}");
            }
        }
    }
}
