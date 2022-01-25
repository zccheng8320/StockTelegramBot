using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAccess;
using Lib.Stock.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lib.CommandProcess
{
    public class ListStockRank : BaseCommandProcessor
    {
        private readonly ILogger<ListStockRank> _logger;
        private readonly IStockRankCrawler _stockRankCrawler;
        private readonly string[] CommandStringAry = new[] { "top_price_diff", "top_volume", "top_gainers", "top_losers", "top_price", "top_turnover" };
        private string _commandName = "";
        public ListStockRank( ILogger<ListStockRank> logger, ITelegramBotClient client, IStockRankCrawler stockRankCrawler) : base(client)
        {
            _logger = logger;
            _stockRankCrawler = stockRankCrawler;
        }

        public override bool IsMatch(Update update)
        {

            var text = update.Message?.Text;
            if (string.IsNullOrEmpty(text)) return false;
            foreach (var s in CommandStringAry)
            {
                if (text.Contains(s))
                {
                    _commandName = s;
                    return true;
                }
            }

            return false;
        }
        
        public override async Task Process(Update update)
        {
            try
            {
                var sortEnum = StockRankSortEnum.FromName(_commandName);
                var stockRank = await _stockRankCrawler.GetStockRank(sortEnum);
                
                var chatId = update.GetChatId();
                var result = new StringBuilder();
                result.AppendLine($"台股{sortEnum.Description}排行");
                result.AppendLine("---");
                foreach (var stockInfo in stockRank.List.OrderBy(m=>m.Rank))
                    result.AppendLine(stockInfo.ToString());
                
                await _client.SendTextMessageAsync(chatId, result.ToString());

            }
            catch (Exception e)
            {
                _logger.LogCritical(e.ToString());
            }
        }
    }
}