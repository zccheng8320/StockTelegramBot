using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Lib
{
    public class HtmlDataDownload : IHtmlDataDownload
    {
        public async Task<byte[]> DownloadDataTask(string url,CancellationToken cancellation)
        {
            var webClient = new WebClient();
            cancellation.Register(webClient.CancelAsync);
            var data = await webClient.DownloadDataTaskAsync(url);
            return data;
        }
    }
}