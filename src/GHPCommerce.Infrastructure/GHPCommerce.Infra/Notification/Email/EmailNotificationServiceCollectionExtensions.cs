using GHPCommerce.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GHPCommerce.Infra.Notification.Email
{
    public static class EmailNotificationServiceCollectionExtensions
    {
        public static IServiceCollection AddSendGridEmailApi(this IServiceCollection services,AuthMessageSenderOptions options)
        {
            _ = services.AddSingleton<IEmailSender>(new EmailSenderService(options.SendGridKey, options.SendGridUser));
            return services;
        }
    }
}
