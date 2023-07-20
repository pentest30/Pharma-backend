using System.Threading.Tasks;

namespace GHPCommerce.Domain.Services
{
    public interface IEmailSender
    {
        Task Execute(string apiKey, string subject, string message, string email);
        Task SendEmailAsync(string email, string subject, string message);
    }
}