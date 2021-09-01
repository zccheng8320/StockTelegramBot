using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lib.CommandProcess
{
    public class SetPortfolio : BaseCommandProcessor
    {
        private readonly ILogger<SetPortfolio> _logger;
        private readonly IStockCodeMapperProvider _stockCodeMapperProvider;
        private readonly Db db;
        private readonly Regex _regex = new("^\\/.?set_portfolio");
        public SetPortfolio(ILogger<SetPortfolio> logger,IDbContextFactory<Db> factory, ITelegramBotClient client, IStockCodeMapperProvider stockCodeMapperProvider) : base(client)
        {
            db = factory.CreateDbContext();
            _logger = logger;
            _stockCodeMapperProvider = stockCodeMapperProvider;
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
                var text = update.Message?.Text;
                if (string.IsNullOrEmpty(text))
                    return;
                if (!TryGetPortfolio(_regex.Replace(text, "").Trim(), out var portfolio))
                {
                    await _client.SendTextMessageAsync(update.GetChatId(), "設定失敗，請確認輸入正確的股票名稱。");
                    return;
                }

                var userId = update.Message.From.Id;
                var user = db.UserPortfolios.FirstOrDefault(m => m.UserId == userId);
                if (user == null)
                {
                    db.UserPortfolios.Add(new UserPortfolio()
                    {
                        UserId = userId,
                        PortfolioSerialize = string.Join(',', portfolio)
                    });
                }
                else
                    user.PortfolioSerialize = string.Join(',', portfolio);
                

                await db.SaveChangesAsync();
                await _client.SendTextMessageAsync(update.GetChatId(), "設定成功!!");
            }
            catch(Exception e)
            {
                _logger.LogCritical(e.ToString());
            }
            finally
            {
                db?.DisposeAsync();
            }
           
        }

        private bool TryGetPortfolio(string text, out List<string> portfolio)
        {
            var codeList = text.Split(" ");
            var dict = _stockCodeMapperProvider.Get();
            portfolio = null;
            for (int i = 0; i < codeList.Length; i++)
            {
                if (!dict.TryGetValue(codeList[i], out string code))
                    return false;
                codeList[i] = code;
            }
            portfolio = codeList.ToList();
            return true;
        }
    }
}