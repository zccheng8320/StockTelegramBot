using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Lib.CommandProcess
{
    public class StxChartSearch :BaseCommandProcessor
    {
        private readonly IStxChartScreenShot _chartScreenShot;
        private readonly IStockCodeMapperProvider _stockCodeMapperProvider;

        public StxChartSearch(ITelegramBotClient client,IStxChartScreenShot chartScreenShot,IStockCodeMapperProvider stockCodeMapperProvider) : base(client)
        {
            _chartScreenShot = chartScreenShot;
            _stockCodeMapperProvider = stockCodeMapperProvider;
        }

        public override async Task Process(Update update)
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
            var photo = await _chartScreenShot.GetChartImageAsync(code);
            var memoryStream = new MemoryStream(photo);
            await _client.SendPhotoAsync(chatId, new InputOnlineFile(memoryStream, $"{userTypingCode}.jpg"));
        }

        private string GetStockCode(Update update)
        {
            var text = update.Message?.Text?.TrimEnd();
            if (text == null)
                return null;
            var regular = new Regex("^\\/.?c");
            return regular.Replace(text, "").Trim();
        }
        public override bool IsMatch(Update update)
        {
            var text = update.Message?.Text;
            if (string.IsNullOrEmpty(text)) return false;
            var regular = new Regex("^\\/.?c");
            return regular.IsMatch(text);
        }
    }
}