using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;


namespace Webhook
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var build = await CreateHostBuilder(args).Build().TelegramStockBotInitialSetting(TelegramGettingUpdatesWay.Webhooks);
            await build.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
