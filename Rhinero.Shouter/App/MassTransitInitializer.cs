using Confluent.Kafka;
using MassTransit;
using Rhinero.Shouter.Consumers;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Shared.Exceptions;
using Rhinero.Shouter.Shared.IBuses;

namespace Rhinero.Shouter.App
{
    internal static class MassTransitInitializer
    {
        internal static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
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
                        x.AddConsumer<ShouterRabbitMQConsumer>();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.UseMessageRetry(r => r.Interval(10, 1)); //TODO: make configurable

                            cfg.Host(rabbitMQConfiguration.Hostname, rabbitMQConfiguration.Port, rabbitMQConfiguration.VirtualHost, h =>
                            {
                                if (!string.IsNullOrWhiteSpace(rabbitMQConfiguration.UserName))
                                    h.Username(rabbitMQConfiguration.UserName);

                                if (!string.IsNullOrWhiteSpace(rabbitMQConfiguration.Password))
                                    h.Password(rabbitMQConfiguration.Password);
                            });

                            cfg.ReceiveEndpoint(rabbitMQConfiguration.Queue, qc =>
                            {
                                qc.SetQueueArgument("x-queue-type", "quorum");
                                qc.Durable = true;
                                qc.AutoStart = true;
                                qc.PrefetchCount = rabbitMQConfiguration.PrefetchCount;
                                qc.ConcurrentMessageLimit = rabbitMQConfiguration.ConcurrentMessageLimit;

                                qc.ConfigureConsumer<ShouterRabbitMQConsumer>(context);
                            });

                        });
                    });
                }

                if (kafkaConfiguration is not null)
                {
                    services.AddMassTransit<IShouterKafkaBus>(x =>
                    {
                        x.UsingInMemory();

                        x.AddRider(r =>
                        {
                            r.AddConsumer<ShouterKafkaConsumer>();

                            r.UsingKafka((context, k) =>
                            {
                                k.Host(kafkaConfiguration.BootstrapServers, h =>
                                {
                                    h.UseSasl(s =>
                                    {
                                        //s.Username = "your-username"; //TODO: add to config
                                        //s.Password = "your-password";
                                        s.Mechanism = SaslMechanism.Plain;
                                    });

                                    h.UseSsl(s =>
                                    {
                                        s.EnableSslCertificateVerification = false; //TODO: add to config
                                    });
                                });

                                k.TopicEndpoint<ShouterMessage>(kafkaConfiguration.Topic, kafkaConfiguration.Group, e =>
                                {
                                    e.PrefetchCount = 16; //TODO: make configurable
                                    e.ConcurrentMessageLimit = 16;
                                    e.ConcurrentDeliveryLimit = 16;
                                    e.ConcurrentConsumerLimit = 16;
                                    e.ConfigureConsumeTopology = true;
                                    e.AutoOffsetReset = AutoOffsetReset.Latest; //TODO: make configurable

                                    e.UseMessageRetry(r => r.Interval(10, 1)); //TODO: make configurable
                                    e.ConfigureConsumer<ShouterKafkaConsumer>(context);
                                    //e.Consumer<ShouterKafkaConsumer>(context);
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
                throw;
            }
        }
    }
}
