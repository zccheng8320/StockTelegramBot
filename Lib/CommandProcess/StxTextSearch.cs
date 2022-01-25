using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lib.CommandProcess
{
    public class StxTextSearch : BaseCommandProcessor
    {
        private readonly ILogger<StxTextSearch> _logger;
        private readonly IStxInfoTextCrawler _stxInfoTextCrawler;
        private readonly IStockCodeMapperProvider _stockCodeMapperProvider;

        public StxTextSearch(ILogger<StxTextSearch> logger, ITelegramBotClient client, IStxInfoTextCrawler stxInfoTextCrawler, IStockCodeMapperProvider stockCodeMapperProvider) : base(client)
        {
            _logger = logger;
            _stxInfoTextCrawler = stxInfoTextCrawler;
            _stockCodeMapperProvider = stockCodeMapperProvider;
        }

        public override async Task Process(Update update)
        {
            try
            {
                var codeMapper = _stockCodeMapperProvider.Get();
                if (string.IsNullOrEmpty(update.Message?.Text?.TrimEnd()))
                    return;
                var userTypingCode = GetStockCode(update);
                var chatId = update.GetChatId();
                if (string.IsNullOrEmpty(userTypingCode))
                    return;
                if (!codeMapper.ContainsKey(userTypingCode))
                {
                    await _client.SendTextMessageAsync(chatId, $"查無{userTypingCode}股票資訊。");
                    return;
                }
                var code = codeMapper[userTypingCode];
                var info = await _stxInfoTextCrawler.GetInfoTextAsync(code);
                await _client.SendTextMessageAsync(chatId, info.ToString());

            }
            catch (Exception e)
            {
                _logger.LogCritical(e.ToString());
            }
        }

        private string GetStockCode(Update update)
        {
            var text = update.Message?.Text?.TrimEnd();
            if (text == null)
                return null;
            var regular = new Regex("^\\/.?t");
            return regular.Replace(text, "").Trim();
        }
        public override bool IsMatch(Update update)
        {
            var text = update.Message?.Text;
            if (string.IsNullOrEmpty(text)) return false;
            if (text.StartsWith("/top", true,CultureInfo.CurrentCulture))
                return false;
            var regular = new Regex("^\\/.?t");
            return regular.IsMatch(text);
        }
    }
}