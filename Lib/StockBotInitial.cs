using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using Lib.SeleniumExtensions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium.Chrome;
using Telegram.Bot;
using TelegramBotExtensions.Interfaces;
using WebCrawler;
using static System.Threading.Tasks.Task;

namespace Lib
{
    public static class StockBotHostExtensions
    {
        public static async Task RunStockTelegramBotAsync(this IHost host, CancellationToken token = default)
        {
            var service = host.Services;
            service.UseDataAccess();
            await LoadStockCodeMapper(service, token);
            if (service.GetService<IStxChartScreenShot>()?.GetType() == typeof(StxChartSelenium))
                await OpenChromeWebDriver(service, token);
            await host.RunAsync(token: token);
        }
        static async Task OpenChromeWebDriver(IServiceProvider serviceProvider, CancellationToken token = default)
        {
            var queue = serviceProvider.GetService<ChromeDriverConcurrentQueue>();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var driverPath = configuration["ChromeDriverSetting:DriverPath"];
            if (string.IsNullOrEmpty(driverPath))
                driverPath = AppDomain.CurrentDomain.BaseDirectory;
            var maxChromeDriverServices = int.Parse(configuration["ChromeDriverSetting:MaxChromeDriverServices"]);
            var tasks = new Task[maxChromeDriverServices];

            for (var i = 0; i < maxChromeDriverServices; i++)
            {
                var task = Run(() =>
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
                }, token);
                tasks[i] = task;
            }
            WaitAll(tasks);
            await CompletedTask;
        }
        static async Task LoadStockCodeMapper(IServiceProvider serviceProvider, CancellationToken token = default)
        {
            var tp = serviceProvider.CreateScope().ServiceProvider.GetService<IStockCodeMapperCreator>();
            await tp.CreateAsync(token);
        }
    }
}