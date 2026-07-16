using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
