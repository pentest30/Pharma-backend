using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace GHPCommerce.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //ThreadStatsLogger logger = new ThreadStatsLogger();
            CreateHostBuilder(args).Build().Run(); 
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                // .UseMetricsWebTracking()
                // .UseMetricsEndpoints(op =>
                // {
                //     op.MetricsTextEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                //     op.MetricsEndpointOutputFormatter = new MetricsPrometheusProtobufOutputFormatter();
                //     op.EnvironmentInfoEndpointEnabled = false;
                // })
                
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog();
    }
}
