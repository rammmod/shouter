using Grpc.Net.Client;
using Rhinero.Shouter.App;
using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Contracts.Payloads.Grpc;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared.Extensions;

namespace Rhinero.Shouter.Services.Callbacks
{
    public class GrpcCallbackService : ICallbackService
    {
        public GrpcCallbackService()
        {
        }

        public async Task SendAsync(ShouterMessage message)
        {
            //var payload = message.Payload.FromJson<GrpcPayload>();

            //using var channel = GrpcChannel.ForAddress(payload.Uri.AbsoluteUri);

            //var client = new Greeter.GreeterClient(channel);

            //var reply = await client.SayHelloAsync(
            //    new HelloRequest()
            //    {
            //        Name = payload.Request
            //    });

            //Console.WriteLine(reply.Message);
        }
    }
}
