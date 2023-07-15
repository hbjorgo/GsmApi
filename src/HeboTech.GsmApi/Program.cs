using HeboTech.ATLib.CodingSchemes;
using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Modems.D_LINK;
using HeboTech.ATLib.Parsers;
using HeboTech.TimeService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO.Ports;

namespace HeboTech.GsmApi
{
    public class Program
    {
        public static readonly SmsTextFormat SmsTextFormat = SmsTextFormat.PDU;
        public static readonly CodingScheme CodingScheme = CodingScheme.UCS2;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configurationBuilder = builder.Configuration.AddCommandLine(args);
            
            TimeService.TimeService.Set(TimeProviders.SystemTimeUtc);

            builder.Services.Configure<GsmConfiguration>(builder.Configuration.GetSection("GsmConfiguration"));

            builder.Services.AddSingleton<IModem>(x =>
                    {
                        var config = x.GetRequiredService<IOptions<GsmConfiguration>>().Value;
                        Console.WriteLine("Gsm Configuration read");
                        Console.WriteLine(config);

                        SerialPort serialPort = new(config.SerialPort, config.BaudRate, Parity.None, 8, StopBits.One)
                        {
                            Handshake = Handshake.RequestToSend
                        };
                        Console.WriteLine("Opening serial port...");
                        serialPort.Open();
                        Console.WriteLine("Serialport opened");

                        AtChannel atChannel = AtChannel.Create(serialPort.BaseStream);
                        DWM222 modem = new(atChannel);
                        atChannel.Open();
                        atChannel.ClearAsync().Wait();

                        ModemResponse echo = modem.DisableEchoAsync().Result;
                        ModemResponse errorFormat = modem.SetErrorFormat(1).Result;
                        var res = modem.SetSmsMessageFormatAsync(SmsTextFormat).Result;

                        ModemResponse<SimStatus> simStatus = modem.GetSimStatusAsync().Result;
                        Console.WriteLine($"SIM Status: {simStatus}");

                        if (simStatus.Result == SimStatus.SIM_PIN)
                        {
                            var simPinStatus = modem.EnterSimPinAsync(new PersonalIdentificationNumber(config.PinCode)).Result;
                            Console.WriteLine($"SIM PIN Status: {simPinStatus}");

                            simStatus = modem.GetSimStatusAsync().Result;
                            Console.WriteLine($"SIM Status: {simStatus}");
                        }

                        var smsTextFormatResult = modem.SetSmsMessageFormatAsync(SmsTextFormat).Result;
                        Console.WriteLine($"Setting SMS text format: {smsTextFormatResult}");

                        var smsIndication = modem.SetNewSmsIndication(2, 1, 0, 0, 0).Result;
                        Console.WriteLine($"SMS indication: {smsIndication}");

                        Console.WriteLine("### Modem initialized ###");

                        return modem;
                    });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            //app.UseHttpsRedirection();

            //app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
