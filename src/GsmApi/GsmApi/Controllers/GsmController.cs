using HeboTech.ATLib.Inputs;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        public IActionResult SendSms([FromBody] SendSmsDto dto)
        {
            SmsReference reference = modem.SendSMS(new PhoneNumber(dto.PhoneNumber), dto.Message);
            if (reference == null)
            {
                _logger.LogError($"Error sending SMS. Phone number: {dto.PhoneNumber}. Message: {dto.Message}");
                return StatusCode(503, "Error sending SMS");
            }
            return new OkObjectResult(reference.MessageReference);
        }
    }
}
