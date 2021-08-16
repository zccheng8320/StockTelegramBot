using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using TelegramBotExtensions.Interfaces;

namespace Lib.SeleniumExtensions
{
    public class ChromeDriverConcurrentQueue : IQueue<ChromeDriver>
    {
        private readonly ConcurrentQueue<ChromeDriver> _chromeDriverQueue;
        private readonly AutoResetEvent _queueNotifier = new AutoResetEvent(false);

        public ChromeDriverConcurrentQueue()
        {
            this._chromeDriverQueue = new ConcurrentQueue<ChromeDriver>();
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
                _queueNotifier.WaitOne();
                if (!_chromeDriverQueue.TryDequeue(out var driver))
                    continue;
                return driver;
            }
        }

        public void Enqueue(ChromeDriver driver)
        {
            _chromeDriverQueue.Enqueue(driver);
            _queueNotifier.Set();
        }
    }
}