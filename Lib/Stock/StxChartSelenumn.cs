using System.Drawing;
using System.Threading.Tasks;
using Lib;
using Lib.SeleniumExtensions;
using OpenQA.Selenium.Chrome;
using TelegramBotExtensions.Interfaces;

namespace WebCrawler
{
    internal class StxChartSelenium:IStxChartScreenShot
    {
        private readonly ChromeDriverConcurrentQueue _chromeDriverProvider;

        public StxChartSelenium(ChromeDriverConcurrentQueue chromeDriverProvider)
        {
            _chromeDriverProvider = chromeDriverProvider;
        }
        private const string 指數網址 = "https://s.yimg.com/nb/tw_stock_frontend/scripts/TseChart/TseChart.eb1b267900.html?sid={0}";
        private const string 個股網址 = "https://s.yimg.com/nb/tw_stock_frontend/scripts/StxChart/StxChart.9d11dfe155.html?sid={0}";
        public async Task<byte[]> GetChartImageAsync(string code)
        {
            return await Task.Run(() =>
            {
                var chromeDriver = _chromeDriverProvider.Dequeue();
                chromeDriver.Manage().Window.Size = new Size(562, 380);
                chromeDriver.Navigate()
                    .GoToUrl(code is Const.櫃買指數代號 or Const.加權指數代號 ? string.Format(指數網址, code) : string.Format(個股網址, code));
                var imageByte = chromeDriver.GetScreenshot().AsByteArray;
                _chromeDriverProvider.Enqueue(chromeDriver);
                return imageByte;
            });
        }
    }
}