using Rhinero.Shouter.Shared.Contracts;
using Rhinero.Shouter.Shared.Json;
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Rhinero.Shouter.Client.Buses;

namespace Rhinero.Shouter.Client
{
    internal sealed class ShouterService : IShouter
    {
        private readonly IServiceProvider _provider;

        public ShouterService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task<Guid> ShoutAsync(Uri uri, ShouterMethodEnums method, string payload, CancellationToken cancellationToken = default, string contentType = "application/json", string token = null, NetworkCredential credentials = null)
        {
            try
            {
                var message = CreateContract(uri, method, payload, contentType, token, credentials);
                await Publish(message, cancellationToken);
                return message.CorrelationId;
            }
            catch (OperationCanceledException)
            {
                return new Guid();
            }
            catch
            {
                throw;
            }
        }

        public Task<Guid> ShoutAsync(string uri, object payload, string token = null, NetworkCredential credentials = null, CancellationToken cancellationToken = default)
        {
            return ShoutAsync(new Uri(uri), payload, token, credentials);
        }

        public Task<Guid> ShoutAsync(Uri uri, object payload, string token = null, NetworkCredential credentials = null, CancellationToken cancellationToken = default)
        {
            return ShoutAsync(uri, ShouterMethodEnums.Post, payload.ToJson(), default, "application/json", token, credentials);
        }

        public Guid Shout(Uri uri, ShouterMethodEnums method, string payload, CancellationToken cancellationToken = default, string contentType = "application/json", string token = null, NetworkCredential credentials = null)
        {
            try
            {
                var message = CreateContract(uri, method, payload, contentType, token, credentials);
                Publish(message, cancellationToken).GetAwaiter().GetResult();
                return message.CorrelationId;
            }
            catch (OperationCanceledException)
            {
                return new Guid();
            }
            catch
            {
                throw;
            }
        }

        private ShouterEvent CreateContract(Uri uri, ShouterMethodEnums method, string payload, string contentType, string token = null, NetworkCredential credentials = null)
        {
            return new ShouterEvent()
            {
                Uri = uri,
                Method = method,
                Payload = payload,
                ContentType = contentType,
                Token = token,
                Credentials = credentials
            };
        }

        private async Task Publish(object message, CancellationToken cancellationToken = default)
        {
            using (var scope = _provider.CreateScope())
            {
                if (cancellationToken == default)
                    cancellationToken = new CancellationTokenSource(new TimeSpan(0, 0, 5)).Token;

                var endpoint = scope.ServiceProvider.GetService<Bind<IShouterRabbitMQBus, IPublishEndpoint>>();
                await endpoint.Value.Publish(message, cancellationToken);
            }
        }

    }
}
