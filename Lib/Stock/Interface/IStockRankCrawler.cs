using System.Threading.Tasks;
using Lib.Stock.Model;

namespace Lib
{
    public interface IStockRankCrawler
    {
        Task<StockRank> GetStockRank(StockRankSortEnum sortEnum);
    }
}