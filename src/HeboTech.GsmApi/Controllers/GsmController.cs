using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems;
using HeboTech.TimeService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeboTech.GsmApi.Controllers
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
                Console.WriteLine($"{TimeService.TimeService.Now} - SMS sent");
                return new OkObjectResult(reference.MessageReference);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error sending SMS";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, errorMessage);
            }
        }

        [HttpGet("Sms")]
        public async Task<IActionResult> ListSms([FromQuery] string senderNumber)
        {
            try
            {
                IList<SmsWithIndex> smss = await modem.ListSmssAsync(SmsStatus.ALL);
                if (senderNumber != null)
                    smss = smss.Where(x => x.Sender.Number == senderNumber).ToList();
                return new OkObjectResult(smss);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error listing SMSs";
                _logger.LogError(ex, errorMessage);
                return StatusCode(503, errorMessage);
            }
        }

        [HttpGet("Sms/{index}")]
        public async Task<IActionResult> ReadSms([FromRoute] int index)
        {
            try
            {
                Sms sms = await modem.ReadSmsAsync(index);
                if (sms == null)
                    return NotFound();

                return new OkObjectResult(new
                {
                    Message = sms.Message,
                    ReceiveTime = sms.ReceiveTime,
                    Sender = sms.Sender.Number,
                });
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error reading SMS";
                _logger.LogError(ex, errorMessage);
                return StatusCode(503, errorMessage);
            }
        }

        [HttpDelete("Sms/{index}")]
        public async Task<IActionResult> DeleteSms([FromRoute] int index)
        {
            try
            {
                CommandStatus status = await modem.DeleteSmsAsync(index);
                if (status == CommandStatus.ERROR)
                    return StatusCode(500, "Unable to delete SMS");

                return NoContent();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error deleting SMS";
                _logger.LogError(ex, errorMessage);
                return StatusCode(503, errorMessage);
            }
        }
    }
}