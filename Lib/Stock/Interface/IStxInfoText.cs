using System.Threading.Tasks;

namespace Lib
{
    public interface IStxInfoTextCrawler
    {
        Task<string> GetInfoTextAsync(string code);
    }
}