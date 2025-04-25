using Google.Protobuf;
using Grpc.Net.Client;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Contracts.Payloads.Grpc;
using Rhinero.Shouter.Helpers;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared.Extensions;
using System.Dynamic;

namespace Rhinero.Shouter.Services.Callbacks
{
    public class GrpcCallbackService : ICallbackService
    {
        private readonly IProtoCache _protoCache;
        private readonly ILogger<GrpcCallbackService> _logger;

        public GrpcCallbackService(
            IProtoCache protoCache,
            ILogger<GrpcCallbackService> logger)
        {
            _protoCache = protoCache;
            _logger = logger;
        }

        public async Task SendAsync(ShouterMessage message, CancellationToken cancellationToken)
        {
            var payload = message.Payload.FromJson<GrpcPayload>();
            var correlationId = message.CorrelationId;

            await CallAsync(payload, correlationId, cancellationToken);
        }

        public async Task<object> ReplyAsync(ShouterRequestMessage message, CancellationToken cancellationToken)
        {
            var payload = message.Payload.FromJson<GrpcPayload>();
            var correlationId = message.CorrelationId;

            return await CallAsync(payload, correlationId, cancellationToken);
        }

        private async Task<object> CallAsync(GrpcPayload payload, Guid correlationId, CancellationToken cancellationToken)
        {
            var (clientType, requestType, replyType) = GrpcHelper.GetMessageTypes(_protoCache, payload);

            using var channel = GrpcChannel.ForAddress(payload.Uri);

            var clientConstructor = clientType.GetConstructor([typeof(Grpc.Core.CallInvoker)]);
            var client = clientConstructor.Invoke([channel.CreateCallInvoker()]);

            var request = Activator.CreateInstance(requestType);
            GrpcHelper.EnrichWithRequestValues(request, payload.RequestParameters);

            var requestMethod = GrpcHelper.GetRequestMethod(clientType, requestType, payload);

            var metadata = GrpcHelper.GetMetadata(payload);

            DateTime? deadline = payload.RequestDeadlineInSeconds is null ?
                null :
                DateTime.UtcNow.AddSeconds((double)payload.RequestDeadlineInSeconds);

            var result = requestMethod.Invoke(client, [request, metadata, deadline, cancellationToken]);

            if (result?.GetType() is null)
            {
                _logger.LogInformation($"Method returned null. Method: {requestMethod.Name}, CorrelationId: {correlationId}");
                return null;
            }

            var response = await GrpcHelper.GetResponseFromInvokationAsync(result);

            if (response is null)
            {
                _logger.LogInformation($"No response received. CorrelationId: {correlationId}");
                return null;
            }

            string json = JsonFormatter.Default.Format(response as IMessage);
            dynamic dynamicObject = json.FromJson<ExpandoObject>();

            return dynamicObject;
        }
    }
}
