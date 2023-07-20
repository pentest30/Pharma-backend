using System.Threading.Tasks;
using GHPCommerce.Domain.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace GHPCommerce.Infra.Notification.Email
{
    public class EmailSenderService : IEmailSender
    {
        private readonly string _sendGridKey;
        private readonly string _sendUser;

        public EmailSenderService(string sendGridKey, string sendUser)
        {
            _sendGridKey = sendGridKey;
            _sendUser = sendUser;
        }

       
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(_sendGridKey, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress("iqbal.baouche@hydrapharmgroupe.com", _sendUser),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);
            var task =  client.SendEmailAsync(msg);
            return task;
        }
    }
}
