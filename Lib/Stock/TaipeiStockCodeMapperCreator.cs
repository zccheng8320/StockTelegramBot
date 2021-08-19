using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Lib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebCrawler.InformationParser
{
    public static class DictionaryExtensions
    {
        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }
            dictionary.Add(key, value);
        }
    }

    public class TaipeiStockCodeMapperCreator : IStockCodeMapperCreator
    {
        private readonly ILogger<TaipeiStockCodeMapperCreator> _logger;
        private readonly IHtmlDataDownload _htmlDataDownload;

        /// <summary>
        /// 上市證卷
        /// </summary>
        private const string mode2Url = "https://isin.twse.com.tw/isin/C_public.jsp?strMode=2";
        /// <summary>
        /// 上櫃證卷
        /// </summary>
        private const string mode4Url = "https://isin.twse.com.tw/isin/C_public.jsp?strMode=4";
        public TaipeiStockCodeMapperCreator(ILogger<TaipeiStockCodeMapperCreator> logger,IHtmlDataDownload htmlDataDownload)
        {
            this._logger = logger;
            _htmlDataDownload = htmlDataDownload;
        }
        public async Task CreateAsync(CancellationToken token)
        {
            var dictionary = new Dictionary<string, string>();
            var url = new[] { mode2Url, mode4Url };
            var crawlerTasks = url.Select(m =>CrawlerCore(m,token)).ToList();
            while (crawlerTasks.Any())
            {
                var finishTask = await Task.WhenAny(crawlerTasks);
                crawlerTasks.Remove(finishTask);
                var result = await finishTask;
                foreach (var (key, value) in result)
                    dictionary.AddOrReplace(key, value);
                finishTask.Dispose();
            }
            dictionary.AddOrReplace("大盤", Const.加權指數代號);
            dictionary.AddOrReplace("加權指數", Const.加權指數代號);
            dictionary.AddOrReplace("加權", Const.加權指數代號);
            dictionary.AddOrReplace("上市", Const.加權指數代號);
            dictionary.AddOrReplace("櫃買指數", Const.櫃買指數代號);
            dictionary.AddOrReplace("櫃買", Const.櫃買指數代號);
            dictionary.AddOrReplace("上櫃", Const.櫃買指數代號);
            _logger.LogInformation($"Saving To {Const.StockCodeMappingInfoPath}");

            var jsonString = JsonConvert.SerializeObject(dictionary);
            await using var stream = File.CreateText(Const.StockCodeMappingInfoPath);
            await stream.WriteAsync(jsonString);
        }

        private async Task<IEnumerable<(string key, string value)>> CrawlerCore(string url,CancellationToken token)
        {
            var data = await _htmlDataDownload.DownloadDataTask(url, token);
            if(token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();
            _logger.LogInformation($"Start parse {url}");
            var doc = new HtmlDocument();
            var str = new StreamReader(new MemoryStream(data), Encoding.GetEncoding(950));
            var html = await str.ReadToEndAsync();
            doc.LoadHtml(html);
            var trNode = doc.DocumentNode.SelectNodes("//table[2]/tr");
            var resultList = new List<(string key, string value)>();
            foreach (var tdCollection in trNode.Select(m => m.SelectNodes("td")))
            {
                if (tdCollection.Count != 7)
                    continue;
                var array = new (string key, string value)[2];
                var codeName = tdCollection[0].InnerHtml;
                var stockInfo = codeName.Split('　');
                if (stockInfo.Length != 2)
                    continue;
                var stockCode = stockInfo[0];
                var stockName = stockInfo[1];
                array[0] = (stockName, stockCode);
                array[1] = (stockCode, stockCode);
                resultList.AddRange(array);
            }
            _logger.LogInformation($"Parse {url} completed");

            return resultList;
        }
    }


}