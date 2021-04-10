using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GsmApi.Controllers
{
    [ApiController]
    [Route("Gsm")]
    public class GsmController : ControllerBase
    {
        private readonly ILogger<GsmController> _logger;
        private readonly IModem modem;

        public GsmController(ILogger<GsmController> logger, IModem modem)
        {
            _logger = logger;
            this.modem = modem;
        }

        [HttpPost("Sms")]
        public async Task<IActionResult> SendSmsAsync([FromBody] SendSmsDto dto)
        {
            try
            {
                SmsReference reference = await modem.SendSmsAsync(new PhoneNumber(dto.PhoneNumber), dto.Message);
                if (reference == null)
                {
                    _logger.LogError($"Error sending SMS. Phone number: {dto.PhoneNumber}. Message: {dto.Message}");
                    return StatusCode(503, "Error sending SMS");
                }
                Console.WriteLine("SMS sent");
                return new OkObjectResult(reference.MessageReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to send SMS");
                return StatusCode(500, "Unable to send SMS");
            }
        }
    }
}
