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
    public class GetPortfolio : BaseCommandProcessor
    {
        private readonly ILogger<GetPortfolio> _logger;
        private readonly IStxInfoTextCrawler _stxInfoTextCrawler;
        private readonly Regex _regex = new("^\\/.?portfolio");
        private readonly Db db;
        public GetPortfolio(IDbContextFactory<Db> factory, ILogger<GetPortfolio> logger, ITelegramBotClient client, IStxInfoTextCrawler stxInfoTextCrawler) : base(client)
        {
            _logger = logger;
            db = factory.CreateDbContext();
            _stxInfoTextCrawler = stxInfoTextCrawler;
        }

        public override bool IsMatch(Update update)
        {
            var text = update.Message?.Text;
            if (string.IsNullOrEmpty(text)) return false;
            return _regex.IsMatch(text);
        }

        public override async Task Process(Update update)
        {
            try
            {
                var userId = update.Message.From.Id;
                var chatId = update.GetChatId();
                var userPortfolio = await db.UserPortfolios.FirstOrDefaultAsync(m => m.UserId == userId);
                var userName = update.Message.From.FirstName + update.Message.From.LastName;
                if (userPortfolio == null)
                {
                    await _client.SendTextMessageAsync(chatId, $"{userName}尚未設定投資組合");
                    return;
                }
                var tasks = userPortfolio.Portfolio.Select(code => _stxInfoTextCrawler.GetInfoTextAsync(code)).ToList();
                var yahooStocks = await Task.WhenAll(tasks);
                var result = new StringBuilder();
                result.AppendLine($"{userName}:投資組合");
                foreach (var yahooStock in yahooStocks)
                {
                    result.AppendLine("－－－－－－－－－－－－－－－－－－－－－－－");
                    result.AppendLine(yahooStock.ToSimpleString());
                }
                await _client.SendTextMessageAsync(chatId, result.ToString());

            }
            catch (Exception e)
            {
                _logger.LogCritical(e.ToString());
            }
            finally
            {
                await db.DisposeAsync();
            }
        }
    }
}