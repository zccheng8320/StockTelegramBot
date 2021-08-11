using System;
using System.Threading.Tasks;
using Lib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramBotExtensions;

namespace LongPolling
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = await CreateHostBuilder(args)
                .Build().TelegramStockBotInitialSetting(TelegramGettingUpdatesWay.LongPolling);

            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddTelegramBotClient();
                    services.AddLongPolling<StxUpdateHandler>();
                    services.AddLib();
                });
        }

    }
}
