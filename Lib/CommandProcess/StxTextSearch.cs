using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

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
                var userTypingCode = update.GetStockCode();
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
                await _client.SendTextMessageAsync(chatId, info);

            }
            catch (Exception e)
            {
                _logger.LogCritical(e.ToString());
            }

        }

        public override bool IsMatch(Update update)
        {
            var text = update.Message?.Text;
            if (string.IsNullOrEmpty(text)) return false;
            if (text.Length < 3) return false;
            return text[0] == '/' && text[1] == 't' && text[2] == ' ';
        }
    }
}