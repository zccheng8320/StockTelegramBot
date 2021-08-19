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
using TelegramBotExtensions;


namespace Webhook
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var build = CreateHostBuilder(args).Build();
            await build.Services.SetWebhookInfo();
            await build.RunStockTelegramBotAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
