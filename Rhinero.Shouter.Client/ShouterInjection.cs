using Confluent.Kafka;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Rhinero.Shouter.Client.Configuration;
using Rhinero.Shouter.Client.Consumers;
using Rhinero.Shouter.Client.Redis;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Shared.Exceptions.Shouter;
using Rhinero.Shouter.Shared.IBuses;
using Rhinero.Shouter.Shared.RetryOptions;
using System.Runtime.Intrinsics.X86;

namespace Rhinero.Shouter.Client
{
    public static class ShouterInjection
    {
        public static IServiceCollection AddShouter(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IShouter, ShouterService>();

            var shouterConfigurationSection = configuration.GetSection(nameof(ShouterConfiguration));

            services.Configure<ShouterConfiguration>(shouterConfigurationSection);
            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<ShouterConfiguration>>().Value);

            var shouterConfiguration = shouterConfigurationSection.Get<ShouterConfiguration>();

            var shouterRabbitMQConfiguration = shouterConfiguration?.RabbitMQ;

            var shouterKafkaConfiguration = shouterConfiguration?.Kafka;

            if (shouterRabbitMQConfiguration is null && shouterKafkaConfiguration is null)
                throw new ShouterBusConfigurationException();

            services.Configure<RetryOptions>(configuration.GetSection("ShouterConfiguration:Retry"));
            services.AddSingleton<IValidateOptions<RetryOptions>, RetryOptionsValidator>();

            if (shouterRabbitMQConfiguration is not null)
            {
                services.AddMassTransit<IShouterRabbitMQBus>(x =>
                {
                    x.AddDelayedMessageScheduler();

                    x.AddRequestClient<ShouterRequestMessage>(
                        new Uri(Shared.Constants.MassTransit.Queue + shouterRabbitMQConfiguration.ReplyQueue),
                        RequestTimeout.After(s: Shared.Constants.ReplyTimeout.LifetimeInt));

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(shouterRabbitMQConfiguration.Hostname, shouterRabbitMQConfiguration.Port, shouterRabbitMQConfiguration.VirtualHost, h =>
                        {
                            if (!string.IsNullOrWhiteSpace(shouterRabbitMQConfiguration.UserName))
                                h.Username(shouterRabbitMQConfiguration.UserName);

                            if (!string.IsNullOrWhiteSpace(shouterRabbitMQConfiguration.Password))
                                h.Password(shouterRabbitMQConfiguration.Password);
                        });

                        cfg.Publish(typeof(ShouterMessage), x =>
                        {
                            x.Durable = true;
                            x.ExchangeType = ExchangeType.Fanout;

                            x.BindQueue(
                                shouterRabbitMQConfiguration.Exchange,
                                shouterRabbitMQConfiguration.PublishQueue,
                                qc =>
                                {
                                    qc.SetQuorumQueue();
                                    qc.Durable = true;
                                    qc.ExchangeType = ExchangeType.Fanout;
                                });
                        });
                    });
                });
            }

            if (shouterKafkaConfiguration is not null)
            {
                services.AddSingleton<IRedisStorage, RedisStorage>();

                services.AddMassTransit<IShouterKafkaBus>(x =>
                {
                    x.UsingInMemory();

                    x.AddDelayedMessageScheduler();

                    x.AddRider(r =>
                    {
                        r.AddConsumer<ShouterKafkaReplyConsumer>();

                        r.AddProducer<ShouterMessage>(shouterKafkaConfiguration.PublishTopic);
                        r.AddProducer<ShouterRequestMessage>(shouterKafkaConfiguration.RequestTopic);

                        r.UsingKafka((context, k) =>
                        {
                            var retryOptions = context.GetRequiredService<IOptions<RetryOptions>>().Value;

                            var sasl = shouterKafkaConfiguration.Sasl;
                            var ssl = shouterKafkaConfiguration.Ssl;

                            k.Host(shouterKafkaConfiguration.BootstrapServers, h =>
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

                            k.TopicEndpoint<ShouterReplyMessage>(shouterKafkaConfiguration.ReplyTopic, shouterKafkaConfiguration.ReplyGroup, e =>
                            {
                                e.PrefetchCount = shouterKafkaConfiguration.PrefetchCount!.Value;
                                e.ConcurrentMessageLimit = shouterKafkaConfiguration.ConcurrentMessageLimit!.Value;
                                e.ConcurrentDeliveryLimit = shouterKafkaConfiguration.ConcurrentDeliveryLimit!.Value;
                                e.ConcurrentConsumerLimit = shouterKafkaConfiguration.ConcurrentConsumerLimit!.Value;
                                e.ConfigureConsumeTopology = true;
                                e.AutoOffsetReset = AutoOffsetReset.Latest;
                                e.CheckpointInterval = TimeSpan.FromSeconds(30);

                                //TODO: add error and skipped topics

                                e.ConfigureRetry(retryOptions);
                                e.ConfigureConsumer<ShouterKafkaReplyConsumer>(context);
                            });
                        });
                    });
                });
            }

            return services;
        }
    }
}
