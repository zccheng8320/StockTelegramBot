using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Lib.Stock.Model;
using Newtonsoft.Json;

namespace Lib
{
    internal class StockRankCrawler : IStockRankCrawler ,IDisposable
    {
        private readonly HttpClient _httpClient;

        public StockRankCrawler(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<StockRank> GetStockRank(StockRankSortEnum sortEnum)
        {
            var urlString = string.Format(_httpClient.BaseAddress.ToString(), sortEnum.SortingCode);
            var uri = new Uri(urlString);
            var responseStream = await _httpClient.GetAsync(urlString);
            responseStream.EnsureSuccessStatusCode();
            var contentString =await responseStream.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StockRank>(contentString);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}