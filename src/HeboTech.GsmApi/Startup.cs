using HeboTech.ATLib.DTOs;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Modems.D_LINK;
using HeboTech.ATLib.Parsers;
using HeboTech.TimeService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.IO.Ports;

namespace HeboTech.GsmApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            TimeService.TimeService.Set(TimeProviders.SystemTimeUtc);
            services.Configure<GsmConfiguration>(Configuration.GetSection("GsmConfiguration"));
            services.AddSingleton<IModem>(x =>
            {
                var config = x.GetRequiredService<IOptions<GsmConfiguration>>().Value;
                Console.WriteLine("Gsm Configuration read");
                Console.WriteLine(config);

                SerialPort serialPort = new SerialPort(config.SerialPort, config.BaudRate, Parity.None, 8, StopBits.One)
                {
                    Handshake = Handshake.RequestToSend
                };
                Console.WriteLine("Opening serial port...");
                serialPort.Open();
                Console.WriteLine("Serialport opened");

                AtChannel atChannel = new(serialPort.BaseStream);
                DWM222 modem = new(atChannel);

                modem.DisableEchoAsync().Wait();

                var simStatus = modem.GetSimStatusAsync().Result;
                Console.WriteLine($"SIM Status: {simStatus}");

                if (simStatus == SimStatus.SIM_PIN)
                {
                    var simPinStatus = modem.EnterSimPinAsync(new PersonalIdentificationNumber(config.PinCode)).Result;
                    Console.WriteLine($"SIM PIN Status: {simPinStatus}");

                    simStatus = modem.GetSimStatusAsync().Result;
                    Console.WriteLine($"SIM Status: {simStatus}");
                }

                var smsTextFormatResult = modem.SetSmsMessageFormatAsync(SmsTextFormat.Text).Result;
                Console.WriteLine($"Setting SMS text format: {smsTextFormatResult}");

                var smsIndication = modem.SetNewSmsIndication(2, 1, 0, 0, 0).Result;
                Console.WriteLine($"SMS indication: {smsIndication}");

                Console.WriteLine("### Modem initialized ###");

                return modem;
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GsmApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GsmApi v1"));

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
