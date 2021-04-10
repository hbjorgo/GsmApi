using Microsoft.AspNetCore.Mvc;
using System;

namespace GsmApi.Controllers
{
    [ApiController]
    [Route("Health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStatus()
        {
            return new OkObjectResult($"Alive @ {DateTime.Now}");
        }
    }
}
