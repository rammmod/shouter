using Microsoft.AspNetCore.Mvc;
using Rhinero.Shouter.Interfaces;

namespace Rhinero.Shouter
{
    [ApiController]
    [Route("[controller]")]
    public class ProtoRegistrationController : ControllerBase
    {
        private readonly IProtoRegistration _protoRegistration;

        public ProtoRegistrationController(
            IProtoRegistration protoRegistration)
        {
            _protoRegistration = protoRegistration;
        }

        [HttpGet("{fileName}")]
        public async Task<ActionResult<KeyValuePair<string, string>>> GetByNameAsync([FromRoute] string fileName) =>
            Ok(await _protoRegistration.GetAsync(fileName, HttpContext.RequestAborted));

        [HttpGet(nameof(GetFileNames))]
        public async Task<ActionResult<ICollection<string>>> GetFileNames() =>
            Ok(await _protoRegistration.GetAllFileNamesAsync(HttpContext.RequestAborted));

        [HttpPost]
        public async Task<IActionResult> AddAsync(IFormFile file)
        {
            await _protoRegistration.AddAsync(file, HttpContext.RequestAborted);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync(IFormFile file)
        {
            await _protoRegistration.PutAsync(file, HttpContext.RequestAborted);
            return Ok();
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string fileName)
        {
            await _protoRegistration.DeleteAsync(fileName, HttpContext.RequestAborted);
            return Ok();
        }
    }
}