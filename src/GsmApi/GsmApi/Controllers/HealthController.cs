using HeboTech.TimeService;
using Microsoft.AspNetCore.Mvc;

namespace GsmApi.Controllers
{
    [ApiController]
    [Route("Health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStatus()
        {
            return new OkObjectResult($"Alive @ {TimeService.Now}");
        }
    }
}
