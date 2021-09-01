using System;
using System.Threading.Tasks;
using Lib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TelegramBotExtensions;
using TelegramBotExtensions.LongPolling;

namespace LongPolling
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunStockTelegramBotAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    var config = logging.Services.BuildServiceProvider().GetService<IConfiguration>();
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddNLog(config);
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddStockTelegramBot(_.Configuration);
                    services.AddTelegramBotClient();
                    services.AddLongPolling<StxUpdateHandler>();
                });
        }

    }
}
