using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HPCS.Service
{

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(o =>
                    {
                      //  o.ListenAnyIP(1883, l => l.UseMqtt()); // MQTT pipeline
                        o.ListenAnyIP(55660); // Default HTTP pipeline
                    });

                });
    }
}