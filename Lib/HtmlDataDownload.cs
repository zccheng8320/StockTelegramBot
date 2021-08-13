using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Lib
{
    public class HtmlDataDownload : IHtmlDataDownload
    {
        private readonly ILogger<HtmlDataDownload> _logger;

        public HtmlDataDownload(ILogger<HtmlDataDownload> htmLogger)
        {
            _logger = htmLogger;
        }
        public async Task<byte[]> DownloadDataTask(string url)
        {
            var webClient = new WebClient();
            var data = await webClient.DownloadDataTaskAsync(url);
            return data;
        }
    }
}