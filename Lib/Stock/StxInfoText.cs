using Lib.Stock.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.Stock
{
    public class StxInfoTextCrawler : IStxInfoTextCrawler
    {
        private readonly ILogger<StxInfoTextCrawler> _logger;
        private readonly IHtmlDataDownload _dataDownload;

        public StxInfoTextCrawler(ILogger<StxInfoTextCrawler> logger, IHtmlDataDownload dataDownload)
        {
            _logger = logger;
            _dataDownload = dataDownload;
        }

        public async Task<YahooStock> GetInfoTextAsync(string code)
        {
            code = code switch
            {
                Const.加權指數代號 => "23001",
                Const.櫃買指數代號 => "23026",
                _ => code
            };
            var url = Const.個股文字現價資訊;
            if (code is "23001" or "23026")
                url = Const.指數文字現價資訊;

            var content = await GetContentAsync(code, url);
            var jsonString = FormatToJson(content);
            var yahooStock = JsonConvert.DeserializeObject<YahooStock>(jsonString);
            yahooStock.Code = code;
            return yahooStock;
        }

        private async Task<string> GetContentAsync(string code, string url)
        {
            var data = await _dataDownload.DownloadDataTask(string.Format(url, code));
            var memory = new MemoryStream(data);
            var streamReader = new StreamReader(memory);
            var content = await streamReader.ReadLineAsync();
            streamReader.Dispose();
            await memory.DisposeAsync();
            return content;
        }

        private string FormatToJson(string content)
        {
            var list = content.ToList();
            var listCount = list.Count;
            list.RemoveRange(listCount - 1 - 1, 2);
            list.RemoveRange(0, 5);
            char? lastChar = null;
            var count = list.Count;
            // Remove lead zero json value
            for (var i = 0; i < count; i++)
            {

                if (i == listCount - 1)
                    break;
                if (lastChar == ':' && list[i] == '0' && list[i + 1] >= '0' && list[i + 1] <= '9')
                {
                    list.RemoveAt(i);
                    count--;
                    i--;
                }
                lastChar = list[i];
            }
            return new string(list.ToArray());
        }
    }

    public enum StockType
    {
        個股,
        指數
    }
}