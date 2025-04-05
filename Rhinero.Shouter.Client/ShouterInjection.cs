using Rhinero.Shouter.Shared.Contracts;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rhinero.Shouter.Client.Buses;
using ShouterConfiguration = Rhinero.Shouter.Client.Configuration.Shouter;

namespace Rhinero.Shouter.Client
{
    public static class ShouterInjection
    {
        public static IServiceCollection AddShouter(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IShouter, ShouterService>();

            var shouterRabbitMQConfiguration =
                configuration.GetSection(nameof(ShouterConfiguration)).Get<ShouterConfiguration>()?.RabbitMQ;

            var shouterKafkaConfiguration =
                configuration.GetSection(nameof(ShouterConfiguration)).Get<ShouterConfiguration>()?.Kafka;

            if (shouterRabbitMQConfiguration is not null)
            {
                services.AddMassTransit<IShouterRabbitMQBus>(x =>
                {
                    x.AddDelayedMessageScheduler();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.UseMessageRetry(r => r.Interval(10, 1));
                        cfg.UseInMemoryOutbox();

                        cfg.Host(shouterRabbitMQConfiguration.Hostname, shouterRabbitMQConfiguration.Port, shouterRabbitMQConfiguration.VirtualHost, h =>
                        {
                            h.Username(shouterRabbitMQConfiguration.UserName);
                            h.Password(shouterRabbitMQConfiguration.Password);
                        });

                        cfg.Publish(typeof(ShouterEvent), x =>
                        {
                            x.Durable = true;
                            x.ExchangeType = ExchangeType.Fanout;

                            x.BindQueue(
                                BuildQueueName(typeof(ShouterEvent).Namespace, nameof(ShouterEvent)),
                                shouterRabbitMQConfiguration.QueueName,
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
                //services.AddMassTransit<IShouterKafkaBus>(x =>
                //{
                //    x.SetKebabCaseEndpointNameFormatter();
                //    x.AddDelayedMessageScheduler();

                //    x.AddRider(rider =>
                //    {
                //        rider.AddProducer<MyKafkaMessage>("kafka-topic-name");
                //        rider.UsingKafka((context, k) =>
                //        {
                //            k.Host("localhost:9092");

                //            k.TopicEndpoint<MyKafkaMessage>("kafka-topic-name", "group-id", e =>
                //            {
                //                e.ConfigureConsumer<MyKafkaConsumer>(context);
                //            });
                //        });
                //    });
                //}); //TODO: add kafka
            }

            return services;
        }

        private static string BuildQueueName(string @namespace, string contractName)
        {
            return @namespace + ":" + contractName;
        }
    }
}
