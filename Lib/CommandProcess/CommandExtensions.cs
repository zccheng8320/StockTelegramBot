using Telegram.Bot.Types;

namespace Lib.CommandProcess
{
    public static class CommandExtensions
    {
        public static string GetStockCode(this Update update)
        {
            var text = update.Message?.Text?.TrimEnd();
            if (text == null)
                return null;
            try
            {
                var userTypingCode = text[3..].Trim();
                return userTypingCode;
            }
            catch
            {
                return null;
            }
        }
        public static long? GetChatId(this Update update)
        {
            return update?.Message?.Chat.Id;
        }
    }
}