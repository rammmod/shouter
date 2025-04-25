using Microsoft.AspNetCore.Mvc;
using Rhinero.Shouter.Client;
using Rhinero.Shouter.Contracts.Enums;
using Rhinero.Shouter.Contracts.Payloads.Grpc;
using Rhinero.Shouter.Contracts.Payloads.Http;
using System.Text.Json;

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

        [HttpPost(nameof(HttpMessage))]
        public async Task<Guid> HttpMessage()
        {
            var body = new
            {
                Id = 1,
                Name = "Art",
                Surname = "Aleksanyan",
                Age = 29
            };

            var httpMessage = new HttpPayload()
            {
                Uri = new Uri("https://webhook.site/7453b0c3-983e-486c-a854-7f98b9499093"),
                Method = HttpMethod.Post,
                Headers = new Dictionary<string, string>()
                {
                    {"Accept", "application/json"},
                    {"User-Agent", "MyApp/1.0" }
                },
                QueryParameters = new Dictionary<string, string>()
                {
                    { "search", "example"},
                    { "limit", "10" }
                },
                Body = JsonSerializer.Serialize(body),
                ContentType = ContentTypeEnum.Json
            };

            return await _shouter.ShoutAsync(Buses.Kafka, Protocol.HTTP, httpMessage, HttpContext.RequestAborted);
        }

        [HttpPost(nameof(GrpcMessage))]
        public async Task<Guid> GrpcMessage()
        {
            var grpcMessage = new GrpcPayload()
            {
                Uri = new Uri("http://localhost:5215"),
                Service = new GrpcService()
                {
                    FileName = "AAA",
                    ClientName = null
                },
                RequestArgumentName = "HelloRequest",
                RequestMethod = "SayHelloAsync",
                RequestParameters = new Dictionary<string, string>()
                {
                    {"Name", "AAA"},
                },
                ResponseArgumentName = "HelloReply",
            };

            return await _shouter.ShoutAsync(Buses.RabbitMQ, Protocol.gRPC, grpcMessage, HttpContext.RequestAborted);
        }
    }
}