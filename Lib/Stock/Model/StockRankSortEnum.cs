using Ardalis.SmartEnum;

namespace Lib.Stock.Model
{
    public class StockRankSortEnum : SmartEnum<StockRankSortEnum>
    {
        public string Description { get; }
        public string SortingCode { get; }
        public static readonly StockRankSortEnum 價差 = new("top_price_diff", 0, "-dayHighLowDiff",nameof(價差));
        public static readonly StockRankSortEnum 成交量 = new("top_volume", 1, "-volume", nameof(成交量));
        public static readonly StockRankSortEnum 漲幅 = new("top_gainers", 2, "-changePercent", nameof(漲幅));
        public static readonly StockRankSortEnum 跌幅 = new("top_losers", 3, "changePercent", nameof(跌幅));
        public static readonly StockRankSortEnum 成交價 = new("top_price", 4, "-price", nameof(成交價));
        public static readonly StockRankSortEnum 成交金額 = new("top_turnover", 5, "-turnoverK", nameof(成交金額));

        private StockRankSortEnum(string name, int value,string sortingCode,string description) : base(name, value)
        {
            Description = description;
            SortingCode = sortingCode;
        }
    }
}