using Rhinero.Shouter.Consumers;
using MassTransit;
using System.Reflection;

namespace Rhinero.Shouter.App
{
    internal static class MassTransitInitializer
    {
        internal static IServiceCollection InitializeMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var mtConfig = configuration.GetSection(nameof(MassTransitConfiguration)).Get<MassTransitConfiguration>();

                var bus = services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    x.AddDelayedMessageScheduler();

                    x.AddConsumers(Assembly.GetEntryAssembly());

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.UseMessageRetry(r => r.Immediate(5));
                        cfg.UseInMemoryOutbox();

                        cfg.Host(mtConfig.Hostname, mtConfig.Port ?? 5672, mtConfig.VirtualHost, h =>
                        {
                            h.Username(mtConfig.UserName);
                            h.Password(mtConfig.Password);
                        });

                        cfg.ReceiveEndpoint(mtConfig.QueueName, qc =>
                        {
                            qc.Durable = true;
                            qc.AutoStart = true;
                            qc.ConfigureConsumeTopology = false;
                            qc.PrefetchCount = mtConfig.PrefetchCount ?? 1;
                            qc.ConcurrentMessageLimit = mtConfig.ConcurrentMessageLimit ?? 1;

                            qc.ConfigureConsumer(context, typeof(ShouterConsumer));
                        });

                    });
                });

                return bus;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
