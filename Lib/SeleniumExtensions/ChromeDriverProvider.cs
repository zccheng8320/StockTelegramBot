using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using TelegramBotExtensions.Interfaces;

namespace Lib.SeleniumExtensions
{
    public class ChromeDriverConcurrentQueue : IQueue<ChromeDriver>
    {
        private readonly IConfiguration _configuration;
        private readonly ConcurrentQueue<ChromeDriver> ChromeDriverQueue;
        public ChromeDriverConcurrentQueue(IConfiguration configuration)
        {
            _configuration = configuration;
            var driverPath = _configuration["ChromeDriverSetting:DriverPath"];
            var maxChromeDriverServices = int.Parse(_configuration["ChromeDriverSetting:MaxChromeDriverServices"]);

            ChromeDriverQueue = new(CreateChromeDriverServices(maxChromeDriverServices, driverPath));
        }

        public ChromeDriverConcurrentQueue(int maxChromeDriverServices, string DriverPath)
        {
            ChromeDriverQueue = new(CreateChromeDriverServices(maxChromeDriverServices, DriverPath));
        }
        private IEnumerable<ChromeDriver> CreateChromeDriverServices(int count, string driverPath)
        {
            var results = new List<ChromeDriver>();
            for (var i = 0; i < count; i++)
            {

                var option = new ChromeOptions();
                option.AddArgument("--headless");
                var driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(driverPath), option);
                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    driver?.Quit();
                    driver?.Dispose();
                };
                yield return driver;
            }
        }
        public ChromeDriver Dequeue()
        {
            while (true)
            {
                if (!ChromeDriverQueue.TryDequeue(out var driver))
                    continue;
                return driver;
            }
        }

        public void Enqueue(ChromeDriver driver)
        {
            ChromeDriverQueue.Enqueue(driver);
        }
    }
}