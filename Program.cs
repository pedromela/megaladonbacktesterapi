using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BacktesterAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //BotEngine.BotEngine.Verbose = false;
            //BotEngine.BotEngine.Logging = false;
            //BrokerLib.BrokerLib.Verbose = false;
            //BrokerLib.BrokerLib.Logging = false;
            //BotLib.BotLib.Verbose = false;
            //BotLib.BotLib.Logging = false;
            //SignalsEngine.SignalsEngine.Logging = false;
            //SignalsEngine.SignalsEngine.Logging = false;
            //UtilsLib.UtilsLib.Verbose = false;
            //UtilsLib.UtilsLib.Logging = false;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
