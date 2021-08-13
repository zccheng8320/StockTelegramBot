using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lib.CommandProcess;
using Lib.CommandProcess.Interfaces;
using Lib.SeleniumExtensions;
using Lib.Stock;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium.Chrome;
using Telegram.Bot;
using TelegramBotExtensions.Interfaces;
using WebCrawler;
using WebCrawler.InformationParser;

namespace Lib
{
    public enum TelegramGettingUpdatesWay
    {
        LongPolling = 0,
        Webhooks = 1
    }
    public static class DependencyInjection
    {
        public static void AddLib(this IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            services.AddMemoryCache();
            //services.AddSingleton<IQueue<ChromeDriver>, ChromeDriverConcurrentQueue>();
            services.AddSingleton<IStockCodeMapperProvider,TaipeiStockCodeMapperProvider>();
            services.AddSingleton<IStockCodeMapperCreator,TaipeiStockCodeMapperCreator>();
            //services.AddSingleton<IStxChartScreenShot, StxChartSelenium>();
            services.AddSingleton<IHtmlDataDownload,HtmlDataDownload>();           
            services.AddSingleton<IStxChartScreenShot, StxChartScreenShot>();
            services.AddScoped<IStxInfoTextCrawler,StxInfoTextCrawler>();           
            services.AddScoped<BaseCommandProcessor, StxChartSearch>();
            services.AddScoped<BaseCommandProcessor, StxTextSearch>();
            services.AddScoped<ICommandProcessorFactory,CommandProcessorFactory>();
        }

        public static async Task<IHost> TelegramStockBotInitialSetting(this IHost host, TelegramGettingUpdatesWay way)
        {
            var services = host.Services;
            if (way == TelegramGettingUpdatesWay.Webhooks)
                await SetWebhookInfo(services);
            else
                await ClearWebhookInfo(services);
            await DownLoadStockInfo(services);
           // await OpenChromeWebDriver(services);
            return host;
        }

        private static async Task ClearWebhookInfo(IServiceProvider services)
        {
            var configure = services.CreateScope().ServiceProvider.GetService<IConfiguration>();
            var telegramBot = services.CreateScope().ServiceProvider.GetService<ITelegramBotClient>();
            await telegramBot.TestApiAsync();
            await telegramBot.SetWebhookAsync("");
        }


        private static async Task OpenChromeWebDriver(IServiceProvider serviceProvider)
        {
            var queue = serviceProvider.GetService<IQueue<ChromeDriver>>();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var driverPath = configuration["ChromeDriverSetting:DriverPath"];
            var maxChromeDriverServices = int.Parse(configuration["ChromeDriverSetting:MaxChromeDriverServices"]);
            var tasks =new Task[maxChromeDriverServices];
            
            for (var i = 0; i < maxChromeDriverServices; i++)
            {
                var task=  Task.Run(() =>
                {
                    var option = new ChromeOptions();
                    option.AddArgument("--headless");
                    var driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(driverPath), option);
                    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                    {
                        driver?.Quit();
                        driver?.Dispose();
                    };
                    queue.Enqueue(driver);
                });
                tasks[i] = task;
            }
            Task.WaitAll(tasks);
            await Task.CompletedTask;
        }
        private static async Task SetWebhookInfo(IServiceProvider serviceProvider)
        {
            var configure = serviceProvider.CreateScope().ServiceProvider.GetService<IConfiguration>();
            var telegramBot = serviceProvider.CreateScope().ServiceProvider.GetService<ITelegramBotClient>();
            await telegramBot.TestApiAsync();
            var telegramWebhookUrl = configure["TelegramSetting:TelegramApiWebhookUrl"];
            await telegramBot.SetWebhookAsync(telegramWebhookUrl);
        }
        private static async Task DownLoadStockInfo(IServiceProvider serviceProvider)
        {
            var tp = serviceProvider.CreateScope().ServiceProvider.GetService<IStockCodeMapperCreator>();
            await tp.CreateAsync();
        }
    }
}