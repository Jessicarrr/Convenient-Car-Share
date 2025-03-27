using Microsoft.AspNetCore.Mvc;

namespace ConvenientCarShare.Controllers
{
    [ApiController]
    [Route("Health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Success - things are running as expected.");
        }
    }
}
