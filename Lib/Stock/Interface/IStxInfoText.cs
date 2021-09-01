using System.Threading.Tasks;
using Lib.Stock.Model;

namespace Lib
{
    public interface IStxInfoTextCrawler
    {
        Task<YahooStock> GetInfoTextAsync(string code);
    }
}