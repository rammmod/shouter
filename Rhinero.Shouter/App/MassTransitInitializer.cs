using Confluent.Kafka;
using MassTransit;
using Microsoft.Extensions.Options;
using Rhinero.Shouter.Consumers;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Shared.Exceptions.Shouter;
using Rhinero.Shouter.Shared.IBuses;
using Rhinero.Shouter.Shared.RetryOptions;

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

                services.Configure<RetryOptions>(configuration.GetSection("Retry"));
                services.AddSingleton<IValidateOptions<RetryOptions>, RetryOptionsValidator>();

                if (rabbitMQConfiguration is not null)
                {
                    services.AddMassTransit<IShouterRabbitMQBus>(x =>
                    {
                        x.AddConsumer<ShouterRabbitMQConsumer>();
                        x.AddConsumer<ShouterRabbitMQReplyConsumer>();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(rabbitMQConfiguration.Hostname, rabbitMQConfiguration.Port, rabbitMQConfiguration.VirtualHost, h =>
                            {
                                if (!string.IsNullOrWhiteSpace(rabbitMQConfiguration.UserName))
                                    h.Username(rabbitMQConfiguration.UserName);

                                if (!string.IsNullOrWhiteSpace(rabbitMQConfiguration.Password))
                                    h.Password(rabbitMQConfiguration.Password);
                            });

                            var retryOptions = context.GetRequiredService<IOptions<RetryOptions>>().Value;

                            cfg.ReceiveEndpoint(rabbitMQConfiguration.PublishQueue, qc =>
                            {
                                qc.SetQueueArgument("x-queue-type", "quorum");
                                qc.Durable = true;
                                qc.AutoStart = true;
                                qc.PrefetchCount = rabbitMQConfiguration.PrefetchCount;
                                qc.ConcurrentMessageLimit = rabbitMQConfiguration.ConcurrentMessageLimit;

                                qc.ConfigureRetry(retryOptions);
                                qc.ConfigureConsumer<ShouterRabbitMQConsumer>(context);
                            });

                            cfg.ReceiveEndpoint(rabbitMQConfiguration.ReplyQueue, qc =>
                            {
                                qc.Durable = false;
                                qc.AutoDelete = true;

                                qc.ConfigureRetry(retryOptions);
                                qc.ConfigureConsumer<ShouterRabbitMQReplyConsumer>(context);
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
                            r.AddConsumer<ShouterKafkaRequestConsumer>();

                            r.AddProducer<ShouterReplyMessage>(kafkaConfiguration.ReplyTopic);

                            r.UsingKafka((context, k) =>
                            {
                                var retryOptions = context.GetRequiredService<IOptions<RetryOptions>>().Value;

                                var sasl = kafkaConfiguration.Sasl;
                                var ssl = kafkaConfiguration.Ssl;

                                k.Host(kafkaConfiguration.BootstrapServers, h =>
                                {
                                    if (sasl is not null)
                                    {
                                        h.UseSasl(s =>
                                        {
                                            if (!string.IsNullOrWhiteSpace(sasl.Username))
                                                s.Username = sasl.Username;

                                            if (!string.IsNullOrWhiteSpace(sasl.Password))
                                                s.Password = sasl.Password;

                                            s.Mechanism = sasl.Mechanism;
                                        });
                                    }

                                    if (ssl is not null)
                                    {
                                        h.UseSsl(s =>
                                        {
                                            s.EnableSslCertificateVerification = ssl.EnableSslCertificateVerification;

                                            if (!string.IsNullOrWhiteSpace(ssl.CaCertificateLocation))
                                                s.CaLocation = ssl.CaCertificateLocation;

                                            if (ssl.UseClientCertificate)
                                            {
                                                if (!string.IsNullOrWhiteSpace(ssl.ClientCertificateLocation))
                                                    s.CertificateLocation = ssl.ClientCertificateLocation;

                                                if (!string.IsNullOrWhiteSpace(ssl.KeystoreLocation))
                                                    s.KeystoreLocation = ssl.KeystoreLocation;

                                                if (!string.IsNullOrWhiteSpace(ssl.KeystorePassword))
                                                    s.KeystorePassword = ssl.KeystorePassword;

                                                if (!string.IsNullOrWhiteSpace(ssl.KeyLocation))
                                                    s.KeyLocation = ssl.KeyLocation;

                                                if (!string.IsNullOrWhiteSpace(ssl.KeyPassword))
                                                    s.KeyPassword = ssl.KeyPassword;
                                            }
                                        });
                                    }
                                });

                                k.TopicEndpoint<ShouterMessage>(kafkaConfiguration.PublishTopic, kafkaConfiguration.PublishGroup, e =>
                                {
                                    e.PrefetchCount = kafkaConfiguration.PrefetchCount!.Value;
                                    e.ConcurrentMessageLimit = kafkaConfiguration.ConcurrentMessageLimit!.Value;
                                    e.ConcurrentDeliveryLimit = kafkaConfiguration.ConcurrentDeliveryLimit!.Value;
                                    e.ConcurrentConsumerLimit = kafkaConfiguration.ConcurrentConsumerLimit!.Value;
                                    e.ConfigureConsumeTopology = true;
                                    e.AutoOffsetReset = AutoOffsetReset.Latest;
                                    e.CheckpointInterval = TimeSpan.FromSeconds(30);

                                    //TODO: add error and skipped topics

                                    e.ConfigureRetry(retryOptions);
                                    e.ConfigureConsumer<ShouterKafkaConsumer>(context);
                                });

                                k.TopicEndpoint<ShouterRequestMessage>(kafkaConfiguration.RequestTopic, kafkaConfiguration.RequestGroup, e =>
                                {
                                    e.PrefetchCount = kafkaConfiguration.PrefetchCount!.Value;
                                    e.ConcurrentMessageLimit = kafkaConfiguration.ConcurrentMessageLimit!.Value;
                                    e.ConcurrentDeliveryLimit = kafkaConfiguration.ConcurrentDeliveryLimit!.Value;
                                    e.ConcurrentConsumerLimit = kafkaConfiguration.ConcurrentConsumerLimit!.Value;
                                    e.ConfigureConsumeTopology = true;
                                    e.AutoOffsetReset = AutoOffsetReset.Latest;
                                    e.CheckpointInterval = TimeSpan.FromSeconds(30);

                                    //TODO: add error and skipped topics

                                    e.ConfigureRetry(retryOptions);
                                    e.ConfigureConsumer<ShouterKafkaRequestConsumer>(context);
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
