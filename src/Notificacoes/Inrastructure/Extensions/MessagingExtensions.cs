using MassTransit;
using Notificacoes.Consumers;

namespace Notificacoes.Infrastructure.Extensions
{
    public static class MessagingExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            var applicationType = configuration["Application:Type"] ?? "LOCAL";

            services.AddMassTransit(x =>
            {
                // Consumers
                x.AddConsumer<UserCreatedEventConsumer>();
                x.AddConsumer<DonationProcessedEventConsumer>();

                x.AddDelayedMessageScheduler();

                var host = configuration["RabbitMq:Host"];
                var user = configuration["RabbitMq:Username"];
                var pass = configuration["RabbitMq:Password"];
                var campaign_created_queue = configuration["QUEUES:CAMPAIGN_CREATED_QUEUE"];
                var donation_processed_queue = configuration["QUEUES:DONATION_PROCESSED_QUEUE"];
                var user_created_queue = configuration["QUEUES:USER_CREATED_QUEUE"];
                logger.LogInformation("user_created_queue: " + donation_processed_queue);

                x.UsingRabbitMq((context, cfg) =>
                {
                    
                    // Mantém na memória do app se o Rabbit cair
                    cfg.UseInMemoryOutbox();

                    cfg.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));

                    
                    // AWS usa URI completa; todos os outros ambientes usam hostname simples
                    if (applicationType == "AWS")
                    {
                        cfg.Host(new Uri(host!), "/", h =>
                        {
                            h.Username(user!);
                            h.Password(pass!);
                        });
                    }
                    else
                    {
                        cfg.Host(host, "/", h =>
                        {
                            h.Username(user!);
                            h.Password(pass!);
                        });
                    }

                   
                    cfg.ReceiveEndpoint(user_created_queue, e =>
                    {
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Tenta 3 vezes a cada 5 seg
                        e.ConfigureConsumer<UserCreatedEventConsumer>(context);
                    });
                    cfg.ReceiveEndpoint(donation_processed_queue, e =>
                    {
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Tenta 3 vezes a cada 5 seg
                        e.ConfigureConsumer<DonationProcessedEventConsumer>(context);
                    });

                    //cfg.ConfigureEndpoints(context);
                });
            });

            // Não bloqueia o startup se o RabbitMQ ainda não estiver disponível
            services.AddOptions<MassTransitHostOptions>().Configure(options =>
            {
                options.WaitUntilStarted = false;
                options.StartTimeout = TimeSpan.FromSeconds(30);
                options.StopTimeout = TimeSpan.FromSeconds(15);
            });

            logger.LogInformation(" ***** Masstransit service inicializado.");

            return services;
        }
    }
}

       