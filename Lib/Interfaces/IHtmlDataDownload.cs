using System.Threading.Tasks;

namespace Lib
{
    public interface IHtmlDataDownload
    {
        Task<byte[]> DownloadDataTask(string url);
    }
}