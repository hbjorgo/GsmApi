using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Parsers;
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
                ModemResponse<SmsReference> response = null;
                switch (Program.SmsTextFormat)
                {
                    case SmsTextFormat.PDU:
                        response = await modem.SendSmsInPduFormatAsync(new PhoneNumber(dto.PhoneNumber), dto.Message, Program.CodingScheme);
                        break;
                    case SmsTextFormat.Text:
                        response = await modem.SendSmsInTextFormatAsync(new PhoneNumber(dto.PhoneNumber), dto.Message);
                        break;
                }
                if (response == null || !response.IsSuccess)
                {
                    _logger.LogError($"Error sending SMS. Errormessage:{response?.ErrorMessage} Phone number: {dto.PhoneNumber}. Message: {dto.Message}");
                    return StatusCode(503, "Error sending SMS");
                }
                Console.WriteLine($"{TimeService.TimeService.Now} - SMS sent");
                return new OkObjectResult(response.Result.MessageReference);
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
                ModemResponse<List<SmsWithIndex>> result = await modem.ListSmssAsync(SmsStatus.ALL);
                List<SmsWithIndex> smss = new();
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
                var sms = await modem.ReadSmsAsync(index, Program.SmsTextFormat);
                if (sms == null || !sms.IsSuccess)
                    return NotFound();

                return new OkObjectResult(new
                {
                    Message = sms.Result.Message,
                    ReceiveTime = sms.Result.ReceiveTime,
                    Sender = sms.Result.Sender.Number,
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
                ModemResponse status = await modem.DeleteSmsAsync(index);
                if (!status.IsSuccess)
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