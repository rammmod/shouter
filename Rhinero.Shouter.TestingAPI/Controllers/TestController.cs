using Rhinero.Shouter.Client;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
            return await _shouter.ShoutAsync(new Uri("https://www.google.com/"), ShouterMethodEnums.Post, "\\{\"asd\": \"asd\"\\}", credentials: new NetworkCredential());
        }
    }
}