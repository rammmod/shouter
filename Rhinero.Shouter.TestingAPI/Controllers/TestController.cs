using Microsoft.AspNetCore.Mvc;
using Rhinero.Shouter.Client;
using Rhinero.Shouter.Contracts.Enums;
using Rhinero.Shouter.Contracts.Payloads.Grpc;
using Rhinero.Shouter.Contracts.Payloads.Http;

namespace Rhinero.Shouter.TestingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IShouter _shouter;

        public TestController(ILogger<TestController> logger, IShouter shouter)
        {
            _logger = logger;
            _shouter = shouter;
        }

        [HttpPost(Name = "PublishToQueue")]
        public async Task<Guid> PublishToQueue()
        {
            var httpMessage = new HttpPayload()
            {
                Uri = new Uri("https://google.com"),
                Method = HttpMethod.Get
            };

            var grpcMessage = new GrpcPayload()
            {

            };

            await _shouter.ShoutAsync(Buses.RabbitMQ, Protocol.HTTP, httpMessage, HttpContext.RequestAborted);
            return await _shouter.ShoutAsync(Buses.RabbitMQ, Protocol.gRPC, grpcMessage, HttpContext.RequestAborted);
        }
    }
}