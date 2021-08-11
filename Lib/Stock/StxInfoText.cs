using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Lib.SeleniumExtensions;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TelegramBotExtensions.Interfaces;

namespace Lib.Stock
{
    public class StxInfoTextCrawler : IStxInfoTextCrawler
    {
        private readonly ILogger<StxInfoTextCrawler> _logger;
        private readonly IQueue<ChromeDriver> _chromeDriverProvider;

        public StxInfoTextCrawler(ILogger<StxInfoTextCrawler> logger,IQueue<ChromeDriver> chromeDriverProvider)
        {
            _logger = logger;
            _chromeDriverProvider = chromeDriverProvider;
        }

        public async Task<string> GetInfoTextAsync(string code)
        {
            return code switch
            {
                Const.加權指數代號 => await new ValueTask<string>(ParseStockInfo("t00").ToString()),
                Const.櫃買指數代號 => await new ValueTask<string>(ParseStockInfo("o00").ToString()),
                _ => await new ValueTask<string>(ParseStockInfo(code).ToString())
            };
        }

        private StockInfo ParseStockInfo(string code)
        {
            var driver = _chromeDriverProvider.Dequeue();
            driver.Navigate().GoToUrl(string.Format(Const.證交所網址,code));
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(d => !string.IsNullOrEmpty(d.FindElement(By.Id($"{code}_n")).Text));
            var n = driver.FindElementById($"{code}_n").Text.Split(' ')[2];
            var v = driver.FindElementById($"{code}_v").Text;
            var z = driver.FindElementById($"{code}_z").Text;
            var diff = decimal.Parse(driver.FindElementById($"{code}_diff").Text[1..]);
            var preStr = driver.FindElementById($"{code}_pre").Text;
            var pre = decimal.Parse(preStr.Substring(1,preStr.Length-3));
            var t = driver.FindElementById($"{code}_t").Text;
            _chromeDriverProvider.Enqueue(driver);
            return new StockInfo()
            {
                Time = t,
                StockCode = code,
                StockName = n,
                TotalVolume = v,
                Price = z,
                QuoteChange = diff,
                PercentOfQuoteChange = pre
            };
        }
    }

    public class StockInfo
    {
        private const string IncreaseIcon = "△ ";
        private const string DecreaseIcon = "▼";
        public string Time { get; set; }
        /// <summary>
        /// 股票名稱
        /// </summary>
        public string StockName { get; set; }
        /// <summary>
        /// 股票代號
        /// </summary>
        public string StockCode { get; set; }
        /// <summary>
        /// 現價
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// 漲跌
        /// </summary>
        public decimal QuoteChange { get; set; }
        /// <summary>
        /// 漲跌幅百分比
        /// </summary>
        public decimal PercentOfQuoteChange { get; set; }
        /// <summary>
        /// 總成交量
        /// </summary>
        public string TotalVolume { get; set; }

        public override string ToString()
        {
            var icon = QuoteChange == 0 ? "" : QuoteChange > 0 ? IncreaseIcon : DecreaseIcon;
            if(this.StockCode == "t00")
                return $"{Time} 加權股價指數 \r\n成交價：{Price} {icon} {QuoteChange} ({PercentOfQuoteChange}%) 總量:{TotalVolume}";
            if (this.StockCode == "o00")
                return $"{Time} 櫃買指數 \r\n成交價：{Price} {icon} {QuoteChange} ({PercentOfQuoteChange}%) 總量:{TotalVolume}";
            return $"{Time} {StockName}({StockCode}) \r\n成交價：{Price} {icon} {QuoteChange} ({PercentOfQuoteChange}%) 總量:{TotalVolume}";
        }
    }
}