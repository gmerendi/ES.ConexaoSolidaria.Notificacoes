using Notificacoes.Domain.Shared.Interface;
using Notificacoes.Infrastructure.Services.Email;

namespace Notificacoes.Infrastructure.Extensions
{
    public static class EmailExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection services, ILogger logger)
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
            logger.LogInformation(" ***** Email service inicializado.");

            return services;
        }
    }
}
