namespace HeboTech.GsmApi
{
    public class GsmConfiguration
    {
        public string SerialPort { get; set; }
        public int BaudRate { get; set; }
        public string PinCode { get; set; }

        public override string ToString()
        {
            return $"Configuration: Port: {SerialPort}, Baudrate: {BaudRate}, PinCode: {PinCode}";
        }
    }
}
