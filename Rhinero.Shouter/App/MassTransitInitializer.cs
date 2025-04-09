using MassTransit;
using Rhinero.Shouter.Consumers;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Shared.Exceptions;
using Rhinero.Shouter.Shared.IBuses;
using System.Reflection;

namespace Rhinero.Shouter.App
{
    internal static class MassTransitInitializer
    {
        internal static IServiceCollection InitializeMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var rabbitMQConfiguration = configuration.GetSection(nameof(RabbitMQ)).Get<RabbitMQ>();
                var kafkaConfiguration = configuration.GetSection(nameof(Kafka)).Get<Kafka>();

                if (rabbitMQConfiguration is null && kafkaConfiguration is null)
                    throw new ShouterBusConfigurationException();

                if (rabbitMQConfiguration is not null)
                {
                    services.AddMassTransit<IShouterRabbitMQBus>(x =>
                    {
                        x.AddDelayedMessageScheduler();

                        x.AddConsumers(Assembly.GetEntryAssembly());

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.UseMessageRetry(r => r.Interval(10, 1)); //TODO: make configurable
                            cfg.UseInMemoryOutbox();

                            cfg.Host(rabbitMQConfiguration.Hostname, rabbitMQConfiguration.Port, rabbitMQConfiguration.VirtualHost, h =>
                            {
                                h.Username(rabbitMQConfiguration.UserName);
                                h.Password(rabbitMQConfiguration.Password);
                            });

                            cfg.ReceiveEndpoint(rabbitMQConfiguration.Queue, qc =>
                            {
                                qc.SetQueueArgument("x-queue-type", "quorum");
                                qc.Durable = true;
                                qc.AutoStart = true;
                                qc.PrefetchCount = rabbitMQConfiguration.PrefetchCount;
                                qc.ConcurrentMessageLimit = rabbitMQConfiguration.ConcurrentMessageLimit;

                                qc.ConfigureConsumer(context, typeof(ShouterRabbitMQConsumer));
                            });

                        });
                    });
                }

                if (kafkaConfiguration is not null)
                {
                    services.AddMassTransit<IShouterKafkaBus>(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();
                        x.AddDelayedMessageScheduler();

                        x.AddRider(rider =>
                        {
                            rider.AddProducer<ShouterMessage>(BuildExchangeName(typeof(ShouterMessage).Namespace, nameof(ShouterMessage)));
                            rider.UsingKafka((context, k) =>
                            {
                                k.Host(kafkaConfiguration.BootstrapServers);

                                k.TopicEndpoint<ShouterMessage>(kafkaConfiguration.Topic, kafkaConfiguration.Group, e =>
                                {
                                    e.ConfigureConsumer<ShouterKafkaConsumer>(context);
                                });
                            });
                        });
                    });
                }

                return services;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private static string BuildExchangeName(string @namespace, string contractName)
        {
            return @namespace + ":" + contractName;
        }
    }
}
