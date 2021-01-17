using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Inputs;
using HeboTech.ATLib.Modems;
using HeboTech.ATLib.Modems.Adafruit;
using HeboTech.ATLib.Parsers;
using HeboTech.ATLib.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.IO.Ports;

namespace GsmApi
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
            services.AddSingleton<IModem>(x =>
            {
                var config = x.GetRequiredService<IOptions<GsmConfiguration>>().Value;

                SerialPort serialPort = new SerialPort(config.SerialPort, config.BaudRate, Parity.None, 8, StopBits.One);
                Console.WriteLine("Opening serial port...");
                serialPort.Open();
                Console.WriteLine("Serialport opened");

                ICommunicator comm = new SerialPortCommunicator(serialPort);
                AtChannel atChannel = new AtChannel(comm);
                AdafruitFona3G modem = new AdafruitFona3G(atChannel);

                modem.DisableEcho();

                var simStatus = modem.GetSimStatus();
                Console.WriteLine($"SIM Status: {simStatus}");

                var remainingCodeAttemps = modem.GetRemainingPinPukAttempts();
                Console.WriteLine($"Remaining attempts: {remainingCodeAttemps}");

                if (simStatus == SimStatus.SIM_PIN)
                {
                    var simPinStatus = modem.EnterSimPin(new PersonalIdentificationNumber(config.PinCode));
                    Console.WriteLine($"SIM PIN Status: {simPinStatus}");

                    simStatus = modem.GetSimStatus();
                    Console.WriteLine($"SIM Status: {simStatus}");
                }

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
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GsmApi v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
