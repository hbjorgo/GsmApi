using System;

namespace GsmApi.Controllers
{
    public class SendSmsDto
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"PhoneNumber: {PhoneNumber}. Message: {Message}.";
        }
    }
}
