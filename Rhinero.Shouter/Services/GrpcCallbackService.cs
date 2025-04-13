using Grpc.Net.Client;
using GrpcGreeter;
using Rhinero.Shouter.App;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Contracts.Payloads.Grpc;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared.Extensions;

namespace Rhinero.Shouter.Services
{
    public class GrpcCallbackService : ICallbackService
    {
        private readonly Protos _protos;

        public GrpcCallbackService(Protos protos)
        {
            _protos = protos;
        }

        public async Task SendAsync(ShouterMessage message)
        {
            var payload = message.Payload.FromJson<GrpcPayload>();

            using var channel = GrpcChannel.ForAddress(payload.Uri.AbsoluteUri);

            var client = new Greeter.GreeterClient(channel);

            var reply = await client.SayHelloAsync(
                new HelloRequest()
                {
                    Name = payload.Request
                });

            Console.WriteLine(reply.Message);
        }
    }
}
