using HeboTech.TimeService;
using Microsoft.AspNetCore.Mvc;

namespace HeboTech.GsmApi.Controllers
{
    [ApiController]
    [Route("Health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStatus()
        {
            return new OkObjectResult($"Alive @ {TimeService.TimeService.Now}");
        }
    }
}
